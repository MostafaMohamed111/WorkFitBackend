using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Documents.CrossCutting.Exceptions;

public sealed class DocumentsUploadFailedException : FeatureException
{
    public DocumentsUploadFailedException()
        : base(
            ModuleMarker.ModuleName,
            "DOCUMENTS_UPLOAD_FAILED",
            "One or more documents were not uploaded successfully. Please try again.",
            "One or more documents could not be attached right now.")
    {
    }
}