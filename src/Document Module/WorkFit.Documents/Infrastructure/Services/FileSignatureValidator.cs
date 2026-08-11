
namespace WorkFit.Documents.Infrastructure.Services;

public static class FileSignatureValidator
{
    public static readonly Dictionary<string, byte[]> FileSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46 } }, // %PDF
        { ".jpg", new byte[] { 0xFF, 0xD8, 0xFF } }, // JPEG
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } }, // JPEG
        { ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47 } }, // PNG
        {  ".docx", new byte[] { 0x50, 0x4B, 0x03, 0x04 }  }, // ZIP (used by DOCX)
    };

    public static bool IsValidFileSignature(Stream fileStream, string extension)
    {
        if (!FileSignatures.TryGetValue(extension, out var expectedSignature))
            return false;
        byte[] fileHeader = new byte[expectedSignature.Length];
        int bytesRead = fileStream.Read(fileHeader, 0, expectedSignature.Length);
        fileStream.Seek(0, SeekOrigin.Begin); // Reset stream position after reading
        if (bytesRead < expectedSignature.Length)
            return false;
        for (int i = 0; i < expectedSignature.Length; i++)
        {
            if (fileHeader[i] != expectedSignature[i])
                return false;
        }
        return true;
    }
}
