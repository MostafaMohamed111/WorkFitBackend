using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Documents.Infrastructure.FileStorage.Exceptions;

public sealed class UnsupportedDocumentExtensionException : FeatureException
{
    public UnsupportedDocumentExtensionException(string extension)
        : base(
            ModuleMarker.ModuleName,
            "UNSUPPORTED_DOCUMENT_EXTENSION",
            $"Document extension '{extension}' is not supported.",
            "The uploaded file type is not supported.")
    {
    }
}