
namespace WorkFit.Documents.Contracts;

public interface IDeleteDocumentService
{
    Task DeleteDocumentsAsync(List<Guid> documentId, CancellationToken cancellationToken);
}
