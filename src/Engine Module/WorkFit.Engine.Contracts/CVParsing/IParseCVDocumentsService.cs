namespace WorkFit.Engine.Contracts.CVParsing;

public interface IParseCVDocumentsService
{
    Task<IReadOnlyList<ParsedCVDocumentResult>> ParseAsync(
        IReadOnlyList<ParsedCVDocumentInput> documents,
        CancellationToken ct = default);
}

public sealed record ParsedCVDocumentInput(
    Guid DocumentId,
    string FileName,
    string ContentType,
    Stream Content);

public sealed record ParsedCVNormalizedSkill(
    Guid SkillId,
    string SkillName,
    int ConfidenceScore,
    string? Evidence,
    string Source);

public sealed record ParsedCVDocumentResult(
    Guid DocumentId,
    bool Success,
    string? Error,
    ParsedCV? ParsedCV,
    IReadOnlyList<ParsedCVNormalizedSkill> NormalizedSkills);
