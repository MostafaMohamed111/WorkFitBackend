using WorkFit.Documents;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Documents.Features.Queries.Exceptions;

public sealed class DocumentContentNotFoundException : FeatureException
{
    public DocumentContentNotFoundException(Guid documentId, string storageKey)
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENT_CONTENT_NOT_FOUND",
            $"Document content was not found in storage. DocumentId: {documentId}, StorageKey: {storageKey}",
            "The document content is unavailable right now.")
    {
    }
}
