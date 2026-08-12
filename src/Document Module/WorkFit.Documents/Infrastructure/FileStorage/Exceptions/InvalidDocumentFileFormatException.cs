using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Documents.Infrastructure.FileStorage.Exceptions;

public sealed class InvalidDocumentFileFormatException : FeatureException
{
    public InvalidDocumentFileFormatException(string extension)
        : base(
            ModuleMarker.ModuleName,
            "INVALID_DOCUMENT_FILE_FORMAT",
            $"File content does not match the expected signature for extension '{extension}'.",
            "The uploaded file content is invalid.")
    {
    }
}