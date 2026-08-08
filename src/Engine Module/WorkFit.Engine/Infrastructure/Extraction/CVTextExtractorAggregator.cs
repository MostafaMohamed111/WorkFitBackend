using Microsoft.Extensions.Logging;

namespace WorkFit.Engine.Infrastructure.Extraction;

public sealed class CVTextExtractorAggregator
{
    private readonly IReadOnlyList<ICVTextExtractor> _extractors;
    private readonly ILogger<CVTextExtractorAggregator> _logger;

    public CVTextExtractorAggregator(IEnumerable<ICVTextExtractor> extractors, ILogger<CVTextExtractorAggregator> logger)
    {
        _extractors = extractors.ToList();
        _logger = logger;
    }

    public async Task<CVExtractionResult> ExtractAsync(string fileName, string mime, Stream content, CancellationToken ct = default)
    {
        var extractor = _extractors.FirstOrDefault(e => e.CanHandle(fileName, mime));
        if (extractor is null)
            return CVExtractionResult.Fail($"Unsupported file type: {fileName} ({mime}). Only PDF and DOCX are supported.");

        try
        {
            var text = await extractor.ExtractAsync(content, ct);
            var trimmed = (text ?? string.Empty).Trim();
            if (trimmed.Length == 0)
                return CVExtractionResult.Fail("Extraction returned empty text (likely a scanned image-only CV).");
            // Hard cap (~32k chars) to keep LLM input bounded.
            const int MaxChars = 32_000;
            if (trimmed.Length > MaxChars)
                trimmed = trimmed[..MaxChars];
            return CVExtractionResult.Ok(trimmed, extractor is PdfTextExtractor ? "PDF" : "DOCX");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Text extraction failed for {FileName}.", fileName);
            return CVExtractionResult.Fail(ex.Message);
        }
    }
}
