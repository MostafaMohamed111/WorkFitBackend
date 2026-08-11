using System.Text.Json;
using Microsoft.Extensions.Logging;
using WorkFit.Engine.Contracts.AI;
using WorkFit.Engine.Contracts.CVParsing;

namespace WorkFit.Engine.Infrastructure.CVParsing;

public interface ICVLLMParser
{
    Task<ParsedCV> ParseAsync(string cvText, CancellationToken ct = default);
}

public sealed class CVLLMParser : ICVLLMParser
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IChatCompletionClient _llm;
    private readonly AIOptions _aiOptions;
    private readonly ILogger<CVLLMParser> _logger;

    public CVLLMParser(IChatCompletionClient llm, Microsoft.Extensions.Options.IOptions<AIOptions> aiOptions, ILogger<CVLLMParser> logger)
    {
        _llm = llm;
        _aiOptions = aiOptions.Value;
        _logger = logger;
    }

    public async Task<ParsedCV> ParseAsync(string cvText, CancellationToken ct = default)
    {
        if (!_aiOptions.Providers.TryGetValue(_aiOptions.DefaultProvider, out var provider))
            throw new InvalidOperationException($"AI provider '{_aiOptions.DefaultProvider}' is not configured.");

        var systemPrompt = BuildSystemPrompt();
        var userPrompt = BuildUserPrompt(cvText);

        var request = new ChatCompletionRequest(
            Model: provider.Model,
            Messages: new[] { new ChatMessage("system", systemPrompt), new ChatMessage("user", userPrompt) },
            Temperature: 0.1,
            ResponseFormatJson: true,
            MaxTokens: _aiOptions.PerJobTokenCap);

        var response = await _llm.SendAsync(request, ct);
        var json = ExtractJsonObject(response.Content);

        var parsed = JsonSerializer.Deserialize<ParsedCV>(json, JsonOptions)
            ?? throw new InvalidOperationException("Deserialized ParsedCV was null.");

        if (!parsed.IsCV)
            return parsed;

        var pIIstripped = StripPII(parsed);
        return ClampScores(pIIstripped);
    }

    private static string BuildSystemPrompt()
    {
        return """
        You are a CV/resume parser. Read the user-provided CV text and return ONLY a JSON object with this exact schema:
        {
          "isCV": boolean,
          "name": string | null,
          "email": string | null,
          "phone": string | null,
          "jobTitle": string | null,
          "summary": string | null,
          "linkedInUrl": string | null,
          "skills": [ { "name": string, "yearsExperience": number | null, "confidenceScore": number (0-100, estimate from depth of evidence), "evidence": string | null } ],
          "experiences": [ { "company": string | null, "role": string | null, "startDate": string | null, "endDate": string | null, "summary": string | null } ],
          "education": [ { "institution": string | null, "degree": string | null, "startDate": string | null, "endDate": string | null } ],
          "certifications": [ { "name": string, "issuer": string | null, "issueDate": string | null, "expiryDate": string | null } ],
          "languages": [ { "name": string, "level": string | null } ],
          "llmConfidence": number (0-100, your overall confidence this is a real CV and the extraction is accurate)
        }
        Rules:
        - Set "isCV":false if the document is clearly NOT a CV (e.g. invoice, photo, contract,blank).
        - Exclude salary, national-id, or any government-issued ID numbers from any field.
        - confidenceScore per skill: ~90 led a team with it; ~85 deep multi-year use; ~70 regular production use; ~50 one project; ~30 mentioned only.
        - Do not duplicate skill variants: collapse "React","React.js","ReactJS","React JS" into a single entry with name "React".
        - If the CV contains a LinkedIn profile URL, set "linkedInUrl" to that URL; otherwise set it to null.
        - Respond with raw JSON only (no markdown fences).
        """;
    }

    private static string BuildUserPrompt(string cvText)
        => $"Extract the structured data from the following CV text.\n\n=== CV TEXT ===\n{cvText}\n";

    private static string ExtractJsonObject(string text)
    {
        var trimmed = (text ?? string.Empty).Trim();
        if (trimmed.StartsWith('{') && trimmed.EndsWith('}')) return trimmed;
        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start >= 0 && end > start) return trimmed[start..(end + 1)];
        throw new InvalidOperationException("LLM response did not contain valid JSON.");
    }

    private static readonly string[] PiiPatterns =
    {
        @"\b\d{3}-\d{2}-\d{4}\b",
        @"\b\d{16}\b",
        @"\b\d{4}\s?\d{4}\s?\d{4}\s?\d{4}\b"
    };

    private static ParsedCV StripPII(ParsedCV cv)
    {
        string? Clean(string? s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            foreach (var p in PiiPatterns) s = System.Text.RegularExpressions.Regex.Replace(s, p, "[REDACTED]");
            return s;
        }
        return cv with
        {
            Name = Clean(cv.Name),
            Email = Clean(cv.Email),
            Phone = Clean(cv.Phone),
            Summary = Clean(cv.Summary),
            JobTitle = Clean(cv.JobTitle),
            LinkedInUrl = Clean(cv.LinkedInUrl)
        };
    }

    private static ParsedCV ClampScores(ParsedCV cv)
    {
        var clamped = cv.Skills
            .Select(s => s with { ConfidenceScore = Math.Clamp(s.ConfidenceScore, 0, 100) })
            .ToList();
        return cv with { Skills = clamped, LLMConfidence = Math.Clamp(cv.LLMConfidence, 0, 100) };
    }
}
