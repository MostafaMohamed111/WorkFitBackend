namespace WorkFit.Documents.Contracts;

public sealed record DocumentMetaDto(
        DateTime UploadedAt,
        string FileName,
        string ContentType,
        long Size
    );
