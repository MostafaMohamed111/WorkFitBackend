using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkFit.Engine.Contracts.AI;

namespace WorkFit.Engine.Infrastructure.AI;

public sealed class MistralChatCompletionClient : IChatCompletionClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<AIOptions> _options;
    private readonly ILogger<MistralChatCompletionClient> _logger;

    public MistralChatCompletionClient(
        IHttpClientFactory httpClientFactory,
        IOptions<AIOptions> options,
        ILogger<MistralChatCompletionClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<ChatCompletionResponse> SendAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default)
    {
        if (!_options.Value.Providers.TryGetValue("Mistral", out var provider) || string.IsNullOrWhiteSpace(provider.ApiKey))
            throw new InvalidOperationException("AI:Providers:Mistral:ApiKey is missing.");

        object payload = new
        {
            model = string.IsNullOrEmpty(request.Model) ? provider.Model : request.Model,
            messages = request.Messages,
            temperature = request.Temperature,
            max_tokens = request.MaxTokens,
            response_format = request.ResponseFormatJson == true ? new { type = "json_object" } : null
        };

        return await RetryAsync(async () =>
        {
            var client = _httpClientFactory.CreateClient("EngineMistral");
            if (client.BaseAddress is null && !string.IsNullOrWhiteSpace(provider.BaseUrl))
                client.BaseAddress = new Uri(provider.BaseUrl, UriKind.Absolute);

            using var req = new HttpRequestMessage(HttpMethod.Post, string.Empty);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", provider.ApiKey);
            req.Headers.TryAddWithoutValidation("User-Agent", "WorkFit.Engine");
            req.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

            using var resp = await client.SendAsync(req, cancellationToken);
            var bodyText = await resp.Content.ReadAsStringAsync(cancellationToken);
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"Mistral {(int)resp.StatusCode} ({resp.StatusCode}). {bodyText}",
                    null,
                    resp.StatusCode);

            using var doc = JsonDocument.Parse(bodyText);
            var content = ExtractMessageContent(doc.RootElement);
            if (string.IsNullOrWhiteSpace(content))
                throw new InvalidOperationException("Mistral response did not contain assistant content.");

            int? promptTokens = null, completionTokens = null;
            string? finish = null;
            if (doc.RootElement.TryGetProperty("usage", out var usageEl))
            {
                if (usageEl.TryGetProperty("prompt_tokens", out var pt) && pt.TryGetInt32(out var ptv)) promptTokens = ptv;
                if (usageEl.TryGetProperty("completion_tokens", out var ct) && ct.TryGetInt32(out var ctv)) completionTokens = ctv;
            }
            if (doc.RootElement.TryGetProperty("choices", out var choicesEl) && choicesEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var ch in choicesEl.EnumerateArray())
                    if (ch.TryGetProperty("finish_reason", out var fr) && fr.ValueKind == JsonValueKind.String)
                        finish = fr.GetString();
            }

            return new ChatCompletionResponse(content, finish, promptTokens, completionTokens);
        }, cancellationToken);
    }

    private async Task<T> RetryAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        Exception? last = null;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try { return await action(); }
            catch (Exception ex) when (attempt < 3 && IsTransient(ex))
            {
                last = ex;
                var delay = TimeSpan.FromSeconds(attempt * 2);
                _logger.LogWarning(ex, "Transient Mistral failure on attempt {Attempt}. Retrying in {Delay}.", attempt, delay);
                await Task.Delay(delay, ct);
            }
        }
        throw last ?? new InvalidOperationException("Mistral request failed.");
    }

    private static bool IsTransient(Exception ex)
    {
        if (ex is TaskCanceledException)
        {
            return true;
        }

        return ex is HttpRequestException http &&
            (http.StatusCode is null ||
             http.StatusCode == System.Net.HttpStatusCode.RequestTimeout ||
             http.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
             (int)http.StatusCode >= 500);
    }

    private static string ExtractMessageContent(JsonElement root)
    {
        if (!root.TryGetProperty("choices", out var choicesEl) || choicesEl.ValueKind != JsonValueKind.Array) return string.Empty;
        foreach (var choice in choicesEl.EnumerateArray())
        {
            if (!choice.TryGetProperty("message", out var msg)) continue;
            if (!msg.TryGetProperty("content", out var contentEl)) continue;
            if (contentEl.ValueKind == JsonValueKind.String) return contentEl.GetString() ?? string.Empty;
            if (contentEl.ValueKind == JsonValueKind.Array)
            {
                var sb = new StringBuilder();
                foreach (var chunk in contentEl.EnumerateArray())
                    if (chunk.TryGetProperty("text", out var t))
                        sb.Append(t.GetString() ?? string.Empty);
                return sb.ToString();
            }
        }
        return string.Empty;
    }
}
