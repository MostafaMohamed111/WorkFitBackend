namespace WorkFit.Documents.Contracts.DocumentLookUpService;

public interface IDocumentLookUpService
{
    Task<IReadOnlyList<DocumentMetaDto>> GetDocumentsByIdsAsync(IReadOnlyList<Guid> documentIds, CancellationToken ct);
}
