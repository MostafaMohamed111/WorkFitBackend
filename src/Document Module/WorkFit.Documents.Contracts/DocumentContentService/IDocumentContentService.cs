namespace WorkFit.Documents.Contracts.DocumentContentService;

public interface IDocumentContentService
{
    Task<DocumentContentDto> OpenReadAsync(Guid documentId, CancellationToken ct = default);
}
