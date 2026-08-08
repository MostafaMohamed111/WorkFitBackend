using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace WorkFit.Engine.Infrastructure.Extraction;

public sealed class PdfTextExtractor : ICVTextExtractor
{
    public bool CanHandle(string fileName, string mime)
        => fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) || mime.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);

    public Task<string> ExtractAsync(Stream content, CancellationToken cancellationToken = default)
    {
        // PdfPig requires a seekable stream (file path or byte array).
        using var ms = new MemoryStream();
        content.CopyTo(ms);
        ms.Position = 0;

        var sb = new System.Text.StringBuilder();
        using var doc = PdfDocument.Open(ms.ToArray());
        foreach (var page in doc.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();
            sb.AppendLine(page.Text);
        }
        return Task.FromResult(sb.ToString());
    }
}

public sealed class DocxTextExtractor : ICVTextExtractor
{
    public bool CanHandle(string fileName, string mime)
        => fileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase)
           || mime.Equals("application/vnd.openxmlformats-officedocument.wordprocessingml.document", StringComparison.OrdinalIgnoreCase);

    public Task<string> ExtractAsync(Stream content, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        content.CopyTo(ms);
        ms.Position = 0;

        using var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(ms, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        var text = body?.InnerText ?? string.Empty;
        return Task.FromResult(text);
    }
}
