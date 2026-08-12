using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Documents.CrossCutting.Exceptions;

public sealed class DocumentsNotFoundException : FeatureException
{
    public DocumentsNotFoundException()
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENTS_NOT_FOUND",
            "One or more documents were not found.",
            "One or more documents could not be found.")
    {
    }
}