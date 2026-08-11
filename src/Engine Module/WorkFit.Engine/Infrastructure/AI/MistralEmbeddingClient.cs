using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkFit.Engine.Contracts.AI;

namespace WorkFit.Engine.Infrastructure.AI;

public sealed class MistralEmbeddingClient : IEmbeddingClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<AIOptions> _options;
    private readonly ILogger<MistralEmbeddingClient> _logger;

    public MistralEmbeddingClient(IHttpClientFactory httpClientFactory, IOptions<AIOptions> options, ILogger<MistralEmbeddingClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<EmbeddingResponse> EmbedAsync(EmbeddingRequest request, CancellationToken cancellationToken = default)
    {
        if (!_options.Value.Providers.TryGetValue("Mistral", out var provider) || string.IsNullOrWhiteSpace(provider.ApiKey))
            throw new InvalidOperationException("AI:Providers:Mistral:ApiKey is missing.");

        var baseUrl = provider.BaseUrl?.Replace("/chat/completions", "/embeddings") ?? "https://api.mistral.ai/v1/embeddings";

        object payload = new
        {
            model = string.IsNullOrEmpty(request.Model) ? (provider.EmbeddingModel ?? "mistral-embed") : request.Model,
            input = request.Inputs
        };

        return await RetryAsync(async () =>
        {
            var client = _httpClientFactory.CreateClient("EngineMistralEmbedding");
            if (client.BaseAddress is null) client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);

            using var req = new HttpRequestMessage(HttpMethod.Post, string.Empty);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", provider.ApiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

            using var resp = await client.SendAsync(req, cancellationToken);
            var bodyText = await resp.Content.ReadAsStringAsync(cancellationToken);
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"Mistral embedding {(int)resp.StatusCode} ({resp.StatusCode}). {bodyText}");

            using var doc = JsonDocument.Parse(bodyText);
            var vectors = new List<ReadOnlyMemory<float>>();
            if (doc.RootElement.TryGetProperty("data", out var dataEl) && dataEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataEl.EnumerateArray())
                {
                    if (item.TryGetProperty("embedding", out var embEl) && embEl.ValueKind == JsonValueKind.Array)
                    {
                        var floats = new List<float>();
                        foreach (var v in embEl.EnumerateArray())
                            floats.Add(v.GetSingle());
                        vectors.Add(new ReadOnlyMemory<float>(floats.ToArray()));
                    }
                }
            }
            return new EmbeddingResponse(vectors);
        }, cancellationToken);
    }

    private async Task<T> RetryAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        Exception? last = null;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try { return await action(); }
            catch (Exception ex) when (attempt < 3 && (ex is HttpRequestException or TaskCanceledException))
            {
                last = ex;
                var delay = TimeSpan.FromSeconds(attempt * 2);
                _logger.LogWarning(ex, "Transient Mistral embedding failure attempt {Attempt}, retrying in {Delay}.", attempt, delay);
                await Task.Delay(delay, ct);
            }
        }
        throw last ?? new InvalidOperationException("Mistral embedding failed.");
    }
}
