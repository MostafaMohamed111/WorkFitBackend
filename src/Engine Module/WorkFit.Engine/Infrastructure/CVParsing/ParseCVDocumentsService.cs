using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.Engine.Infrastructure.Extraction;

namespace WorkFit.Engine.Infrastructure.CVParsing;

public sealed class ParseCVDocumentsService : IParseCVDocumentsService
{
    private readonly CVTextExtractorAggregator _extractor;
    private readonly ICVLLMParser _parser;
    private readonly ICVSkillNormalizer _skillNormalizer;

    public ParseCVDocumentsService(
        CVTextExtractorAggregator extractor,
        ICVLLMParser parser,
        ICVSkillNormalizer skillNormalizer)
    {
        _extractor = extractor;
        _parser = parser;
        _skillNormalizer = skillNormalizer;
    }

    public async Task<IReadOnlyList<ParsedCVDocumentResult>> ParseAsync(
        IReadOnlyList<ParsedCVDocumentInput> documents,
        CancellationToken ct = default)
    {
        var results = new List<ParsedCVDocumentResult>(documents.Count);

        foreach (var document in documents)
        {
            ct.ThrowIfCancellationRequested();

            if (document.Content.CanSeek)
                document.Content.Position = 0;

            var extraction = await _extractor.ExtractAsync(
                document.FileName,
                document.ContentType,
                document.Content,
                ct);

            if (!extraction.Success)
            {
                results.Add(new ParsedCVDocumentResult(
                    document.DocumentId,
                    false,
                    extraction.Error,
                    null,
                    Array.Empty<ParsedCVNormalizedSkill>()));
                continue;
            }

            var parsed = await _parser.ParseAsync(extraction.Text ?? string.Empty, ct);
            if (!parsed.IsCV)
            {
                results.Add(new ParsedCVDocumentResult(
                    document.DocumentId,
                    false,
                    "Uploaded document was not detected as a CV.",
                    parsed,
                    Array.Empty<ParsedCVNormalizedSkill>()));
                continue;
            }

            var normalized = await _skillNormalizer.NormalizeAsync(parsed.Skills, ct);
            var normalizedSkills = normalized
                .Select(s => new ParsedCVNormalizedSkill(
                    s.SkillId,
                    s.SkillName,
                    s.ConfidenceScore,
                    s.Evidence,
                    s.Source))
                .ToList();

            results.Add(new ParsedCVDocumentResult(
                document.DocumentId,
                true,
                null,
                parsed,
                normalizedSkills));
        }

        return results;
    }
}
