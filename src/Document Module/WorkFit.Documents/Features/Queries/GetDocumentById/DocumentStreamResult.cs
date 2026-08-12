namespace WorkFit.Documents.Features.Queries.GetDocumentById;

public sealed record DocumentStreamResult(
    Stream Content,
    string ContentType,
    string FileName
);