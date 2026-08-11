using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Documents.Infrastructure.FileStorage.Exceptions;

public sealed class FileExceedsMaximumSizeException : FeatureException
{
    public FileExceedsMaximumSizeException(long maxBytes, long actualBytes)
        : base(
            ModuleMarker.ModuleName,
            "FILE_EXCEEDS_MAXIMUM_SIZE",
            $"Uploaded file size {actualBytes} bytes exceeds the maximum allowed size of {maxBytes} bytes.",
            "The uploaded file is too large.")
    {
    }
}