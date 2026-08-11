namespace WorkFit.Engine.Contracts.AI;

public sealed record AIProviderOptions(
    string Provider,
    string Model,
    string? ApiKey,
    string? BaseUrl,
    string? EmbeddingModel = null,
    int TimeoutSeconds = 120,
    int MaxRetries = 3);

public sealed class AIOptions
{
    public string DefaultProvider { get; init; } = "Mistral";
    public int MonthlyTokenCapPerOrg { get; init; } = 2_000_000;
    public int PerJobTokenCap { get; init; } = 8_000;
    public Dictionary<string, AIProviderOptions> Providers { get; init; } = new();
}
