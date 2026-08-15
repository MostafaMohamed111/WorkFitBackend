namespace WorkFit.Documents.Contracts.TemporaryUploadService;

public sealed record CreatedTemporaryDocumentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long Size);
