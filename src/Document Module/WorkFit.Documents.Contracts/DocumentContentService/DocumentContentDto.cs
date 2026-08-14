namespace WorkFit.Documents.Contracts.DocumentContentService;

public sealed record DocumentContentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long Size,
    Stream Content);
