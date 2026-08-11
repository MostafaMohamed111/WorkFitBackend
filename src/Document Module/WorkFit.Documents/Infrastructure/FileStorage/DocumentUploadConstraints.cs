using WorkFit.Documents.Infrastructure.FileStorage.Exceptions;
using WorkFit.Documents.Infrastructure.Services;

namespace WorkFit.Documents.Infrastructure.FileStorage;

/// <summary>Shared rules for document uploads (local disk or blob).</summary>
internal static class DocumentUploadConstraints
{
    public const long MaxBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".docx"
    };

    /// <summary>Validates size, extension, and magic bytes; leaves <paramref name="content"/> positioned at the start.</summary>
    public static string ValidateAndPrepareStream(Stream content, string fileName)
    {
        if (content.Length > MaxBytes)
            throw new FileExceedsMaximumSizeException(MaxBytes, content.Length);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            throw new UnsupportedDocumentExtensionException(extension);

        if (!FileSignatureValidator.IsValidFileSignature(content, extension))
            throw new InvalidDocumentFileFormatException(extension);

        if (content.CanSeek)
            content.Position = 0;

        return extension;
    }

    public static string BuildStorageKey(string extension) =>
        $"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}{extension}";
}
