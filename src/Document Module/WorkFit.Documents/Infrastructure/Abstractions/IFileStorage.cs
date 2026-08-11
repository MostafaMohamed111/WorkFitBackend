

namespace WorkFit.Documents.Infrastructure.Abstractions;

public interface IFileStorage
{
    Task<string> UplaodDocumentAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct);

    Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct);

    Task DeleteAsync(string storageKey, CancellationToken ct);

}
