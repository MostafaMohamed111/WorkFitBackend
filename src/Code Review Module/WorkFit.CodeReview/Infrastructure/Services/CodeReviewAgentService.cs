using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkFit.CodeReview.Contracts.GitHubCodeReview;
using WorkFit.CodeReview.Infrastructure.Options;
using WorkFit.CodeReview.Infrastructure.Services.Models;

namespace WorkFit.CodeReview.Infrastructure.Services;

public sealed class CodeReviewAgentService : ICodeReviewAgentService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<CodeReviewOptions> _options;
    private readonly ILogger<CodeReviewAgentService> _logger;

    public CodeReviewAgentService(
        IHttpClientFactory httpClientFactory,
        IOptions<CodeReviewOptions> options,
        ILogger<CodeReviewAgentService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<CodeReviewReviewerResult> ReviewAsync(
        CodeReviewReviewerConfig reviewer,
        string repository,
        string commitSha,
        string codeContext,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(codeContext))
        {
            return new CodeReviewReviewerResult(
                reviewer.ReviewerKey,
                reviewer.ReviewerName,
                repository,
                commitSha,
                null,
                Array.Empty<CodeReviewIssueDto>(),
                [ "No reviewable code context provided" ],
                Array.Empty<string>());
        }

        var prompt = BuildReviewPrompt(repository, commitSha, codeContext);
        var payload = new
        {
            model = _options.Value.Mistral.Model,
            messages = new object[]
            {
                new { role = "system", content = BuildSystemPrompt(reviewer) },
                new { role = "user", content = prompt }
            },
            temperature = 0.2
        };

        var text = await SendChatCompletionAsync(payload, ct);
        var parsed = ParseJsonObject(text);

        var score = reviewer.Scored && parsed.TryGetProperty("score", out var scoreElement) && scoreElement.TryGetInt32(out var parsedScore)
            ? Math.Clamp(parsedScore, 0, 100)
            : (int?)null;

        var issues = new List<CodeReviewIssueDto>();
        if (parsed.TryGetProperty("issues", out var issuesElement) && issuesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var issue in issuesElement.EnumerateArray())
            {
                issues.Add(new CodeReviewIssueDto(
                    issue.TryGetProperty("title", out var titleElement) ? titleElement.GetString() ?? string.Empty : string.Empty,
                    issue.TryGetProperty("severity", out var severityElement) ? severityElement.GetString() ?? "medium" : "medium",
                    issue.TryGetProperty("detail", out var detailElement) ? detailElement.GetString() ?? string.Empty : string.Empty,
                    issue.TryGetProperty("recommendation", out var recommendationElement) ? recommendationElement.GetString() ?? string.Empty : string.Empty,
                    issue.TryGetProperty("file", out var fileElement) ? fileElement.GetString() ?? string.Empty : string.Empty,
                    reviewer.ReviewerKey));
            }
        }

        var recommendations = ExtractStringArray(parsed, "recommendations");
        var positiveFindings = ExtractStringArray(parsed, "positiveFindings");

        return new CodeReviewReviewerResult(
            reviewer.ReviewerKey,
            reviewer.ReviewerName,
            repository,
            commitSha,
            score,
            issues,
            recommendations,
            positiveFindings);
    }

    public async Task<CodeReviewSummaryResult> GenerateSummariesAsync(object aggregatedReviewData, CancellationToken ct)
    {
        var prompt = $"""
You are a senior engineering reviewer. Based on the aggregated multi-agent code review data below, produce two summaries.

Rules:
- executiveSummary: suitable for a Team Lead, MAXIMUM 10 sentences, covers overall quality, risk, and the most important issues.
- developerSummary: friendly, actionable, and concrete; reference specific findings and next steps.

AGGREGATED REVIEW DATA (JSON):
{JsonSerializer.Serialize(aggregatedReviewData, JsonOptions)}
""";

        var payload = new
        {
            model = _options.Value.Mistral.Model,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "Return only valid JSON with executiveSummary and developerSummary."
                },
                new { role = "user", content = prompt }
            },
            temperature = 0.2
        };

        var text = await SendChatCompletionAsync(payload, ct);
        var parsed = ParseJsonObject(text);

        return new CodeReviewSummaryResult(
            parsed.TryGetProperty("executiveSummary", out var executiveSummaryElement) ? executiveSummaryElement.GetString() ?? string.Empty : string.Empty,
            parsed.TryGetProperty("developerSummary", out var developerSummaryElement) ? developerSummaryElement.GetString() ?? string.Empty : string.Empty);
    }

    private async Task<string> SendChatCompletionAsync(object payload, CancellationToken ct)
    {
        var options = _options.Value.Mistral;
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new InvalidOperationException("CodeReview:Mistral:ApiKey is missing.");
        }

        var client = _httpClientFactory.CreateClient("CodeReviewMistral");
        client.BaseAddress ??= new Uri(options.BaseUrl, UriKind.Absolute);

        return await RetryAsync(async () =>
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, string.Empty);
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiKey);
            request.Headers.TryAddWithoutValidation("User-Agent", options.UserAgent);
            request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"Mistral request failed with status {(int)response.StatusCode} ({response.StatusCode}). {body}");
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            var content = ExtractMessageContent(doc.RootElement);
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("Mistral response did not contain assistant content.");
            }

            return content;
        }, ct);
    }

    private async Task<T> RetryAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (attempt < 3 && IsTransient(ex))
            {
                lastException = ex;
                var delay = TimeSpan.FromSeconds(attempt * 2);
                _logger.LogWarning(ex, "Transient Mistral failure on attempt {Attempt}. Retrying in {Delay}.", attempt, delay);
                await Task.Delay(delay, ct);
            }
        }

        throw lastException ?? new InvalidOperationException("Mistral request failed.");
    }

    private static bool IsTransient(Exception ex)
    {
        return ex is HttpRequestException or TaskCanceledException;
    }

    private static string BuildSystemPrompt(CodeReviewReviewerConfig reviewer)
    {
        var basePrompt = "You are the " + reviewer.ReviewerName + ", an expert .NET/C# code reviewer. Focus areas: " + reviewer.Focus + ". ";
        basePrompt += "Analyze ONLY the provided diff. Respond with a JSON object containing score (integer 0-100), issues (array of objects with title, severity one of critical|high|medium|low, detail, recommendation, file), recommendations (array of strings), positiveFindings (array of strings). Be specific and concise.";

        if (!reviewer.Scored)
        {
            basePrompt += " Do NOT include a numeric score; set positiveFindings only.";
        }

        return basePrompt;
    }

    private static string BuildReviewPrompt(string repository, string commitSha, string codeContext)
    {
        return $"""
Review the following code changes for repository {repository} at commit {commitSha}.

=== CODE CONTEXT ===
{codeContext}
""";
    }

    private static JsonElement ParseJsonObject(string text)
    {
        var candidate = ExtractJsonObject(text);
        using var doc = JsonDocument.Parse(candidate);
        return doc.RootElement.Clone();
    }

    private static string ExtractJsonObject(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.StartsWith("{", StringComparison.Ordinal) && trimmed.EndsWith("}", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            return trimmed[start..(end + 1)];
        }

        throw new InvalidOperationException("AI response did not contain valid JSON.");
    }

    private static IReadOnlyList<string> ExtractStringArray(JsonElement parent, string propertyName)
    {
        if (!parent.TryGetProperty(propertyName, out var element) || element.ValueKind != JsonValueKind.Array)
            return Array.Empty<string>();

        var values = new List<string>();
        foreach (var item in element.EnumerateArray())
        {
            var value = ReadStringValue(item);
            if (!string.IsNullOrWhiteSpace(value))
            {
                values.Add(value);
            }
        }

        return values;
    }

    private static string ExtractMessageContent(JsonElement root)
    {
        if (!root.TryGetProperty("choices", out var choicesElement) || choicesElement.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        foreach (var choice in choicesElement.EnumerateArray())
        {
            if (!choice.TryGetProperty("message", out var messageElement))
                continue;

            if (!messageElement.TryGetProperty("content", out var contentElement))
                continue;

            if (contentElement.ValueKind == JsonValueKind.String)
                return contentElement.GetString() ?? string.Empty;

            if (contentElement.ValueKind == JsonValueKind.Array)
            {
                var sb = new StringBuilder();
                foreach (var chunk in contentElement.EnumerateArray())
                {
                    if (chunk.TryGetProperty("text", out var textElement))
                    {
                        sb.Append(ReadStringValue(textElement));
                    }
                }
                return sb.ToString();
            }
        }

        return string.Empty;
    }

    private static string ReadStringValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Object => element.GetRawText(),
            JsonValueKind.Array => element.GetRawText(),
            _ => string.Empty
        };
    }
}
