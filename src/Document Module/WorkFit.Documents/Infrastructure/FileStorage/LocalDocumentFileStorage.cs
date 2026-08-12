using WorkFit.Documents.Infrastructure.Abstractions;
using WorkFit.Documents.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace WorkFit.Documents.Infrastructure.FileStorage;

public sealed class LocalDocumentFileStorage : IFileStorage
{
    private readonly string _rootFull;
    private readonly string _rootPrefix;

    public LocalDocumentFileStorage(
        IOptions<LocalDocumentFileStorageOptions> options,
        IHostEnvironment hostEnvironment)
    {
        var configured = options.Value.RootPath?.Trim() ?? string.Empty;
        var root = string.IsNullOrEmpty(configured)
            ? ResolveDefaultRoot(hostEnvironment.ContentRootPath)
            : Path.GetFullPath(configured);

        _rootFull = Path.GetFullPath(root);
        Directory.CreateDirectory(_rootFull);

        _rootPrefix = _rootFull.EndsWith(Path.DirectorySeparatorChar)
            ? _rootFull
            : _rootFull + Path.DirectorySeparatorChar;
    }

    public async Task<string> UplaodDocumentAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct)
    {
        _ = contentType;
        var extension = DocumentUploadConstraints.ValidateAndPrepareStream(content, fileName);
        var storageKey = DocumentUploadConstraints.BuildStorageKey(extension);
        var fullPath = ResolveExistingKeyToPath(storageKey);

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await using var output = new FileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            8192,
            useAsync: true);

        await content.CopyToAsync(output, ct).ConfigureAwait(false);

        return storageKey;
    }

    public Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct)
    {
        _ = ct;
        var fullPath = ResolveExistingKeyToPath(storageKey);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found.", storageKey);

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            8192,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct)
    {
        _ = ct;
        var fullPath = ResolveExistingKeyToPath(storageKey);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    private string ResolveExistingKeyToPath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey)
            || storageKey.Contains("..", StringComparison.Ordinal)
            || storageKey.Contains('\\'))
        {
            throw new ArgumentException("Invalid storage key.", nameof(storageKey));
        }

        var segments = storageKey.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var candidate = Path.GetFullPath(Path.Combine(new[] { _rootFull }.Concat(segments).ToArray()));

        if (!candidate.StartsWith(_rootPrefix, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(candidate, _rootFull, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid storage key.", nameof(storageKey));
        }

        return candidate;
    }

    private static string ResolveDefaultRoot(string contentRoot) =>
        OperatingSystem.IsWindows()
            ? Path.GetFullPath(@"C:\expatzi-storage\documents")
            : Path.Combine(contentRoot, "expatzi-storage", "documents");
}
