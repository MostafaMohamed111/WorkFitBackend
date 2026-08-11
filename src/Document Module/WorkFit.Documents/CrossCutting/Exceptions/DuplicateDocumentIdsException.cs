using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Documents.CrossCutting.Exceptions;

public sealed class DuplicateDocumentIdsException : FeatureException
{
    public DuplicateDocumentIdsException()
        : base(
            ModuleMarker.ModuleName,
            "DUPLICATE_DOCUMENT_IDS",
            "One or more document IDs were duplicated.",
            "Duplicate document IDs were provided.")
    {
    }
}