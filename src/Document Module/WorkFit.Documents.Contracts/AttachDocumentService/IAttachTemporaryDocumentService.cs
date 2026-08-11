

namespace WorkFit.Documents.Contracts.AttachDocumentService;

public interface IAttachTemporaryDocumentService
{
    Task<IReadOnlyDictionary<Guid, DocumentMetaDto>> AttachDocumentsByIdsAsync(IReadOnlyList<Guid> DocumentIds, Guid ownerId, CancellationToken ct);
}
