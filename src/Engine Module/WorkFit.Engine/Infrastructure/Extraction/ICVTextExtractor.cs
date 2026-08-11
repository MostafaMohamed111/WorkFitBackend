namespace WorkFit.Engine.Infrastructure.Extraction;

public interface ICVTextExtractor
{
    bool CanHandle(string fileName, string mime);
    Task<string> ExtractAsync(Stream content, CancellationToken cancellationToken = default);
}

public sealed class CVExtractionResult
{
    public bool Success { get; init; }
    public string Text { get; init; } = string.Empty;
    public string? Error { get; init; }
    public string Source { get; init; } = string.Empty;

    public static CVExtractionResult Ok(string text, string source) => new() { Success = true, Text = text, Source = source };
    public static CVExtractionResult Fail(string error) => new() { Success = false, Error = error };
}
