using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkFit.Engine.Contracts.AI;

namespace WorkFit.Engine.Infrastructure.AI;

public sealed class GeminiEmbeddingClient(
    IHttpClientFactory httpClientFactory,
    IOptions<AIOptions> options,
    ILogger<GeminiEmbeddingClient> logger) : IEmbeddingClient
{
    private const string DefaultModel = "gemini-embedding-2";

    public async Task<EmbeddingResponse> EmbedAsync(
        EmbeddingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Inputs);
        if (request.Inputs.Count == 0 || request.Inputs.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("At least one non-empty embedding input is required.", nameof(request));

        if (!options.Value.Providers.TryGetValue("Gemini", out var provider) ||
            string.IsNullOrWhiteSpace(provider.ApiKey))
        {
            throw new InvalidOperationException("AI:Providers:Gemini:ApiKey is missing.");
        }

        var model = NormalizeModel(string.IsNullOrWhiteSpace(request.Model)
            ? provider.EmbeddingModel ?? DefaultModel
            : request.Model);
        var dimensions = request.OutputDimensionality ?? 1024;
        if (dimensions is < 128 or > 3072)
            throw new ArgumentOutOfRangeException(nameof(request), "Gemini embedding dimensions must be between 128 and 3072.");

        var vectors = new List<ReadOnlyMemory<float>>(request.Inputs.Count);
        foreach (var input in request.Inputs)
        {
            vectors.Add(await EmbedOneAsync(
                provider.ApiKey,
                model,
                input,
                request.TaskType,
                dimensions,
                cancellationToken));
        }

        return new EmbeddingResponse(vectors);
    }

    private async Task<ReadOnlyMemory<float>> EmbedOneAsync(
        string apiKey,
        string model,
        string input,
        EmbeddingTaskType taskType,
        int dimensions,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            model = $"models/{model}",
            content = new
            {
                role = "user",
                parts = new[] { new { text = input } }
            },
            embedContentConfig = new
            {
                taskType = ToApiTaskType(taskType),
                outputDimensionality = dimensions,
                autoTruncate = true
            }
        };

        return await RetryAsync(async () =>
        {
            var client = httpClientFactory.CreateClient("EngineGeminiEmbedding");
            using var message = new HttpRequestMessage(
                HttpMethod.Post,
                $"models/{Uri.EscapeDataString(model)}:embedContent");
            message.Headers.Add("x-goog-api-key", apiKey);
            message.Content = JsonContent.Create(payload);

            using var response = await client.SendAsync(message, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Gemini embedding {(int)response.StatusCode} ({response.StatusCode}). {body}",
                    null,
                    response.StatusCode);
            }

            using var document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty("embedding", out var embedding) ||
                !embedding.TryGetProperty("values", out var values) ||
                values.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidOperationException("Gemini embedding response did not contain embedding.values.");
            }

            var vector = values.EnumerateArray().Select(value => value.GetSingle()).ToArray();
            if (vector.Length != dimensions)
                throw new InvalidOperationException($"Gemini returned {vector.Length} dimensions; expected {dimensions}.");

            Normalize(vector);
            return new ReadOnlyMemory<float>(vector);
        }, cancellationToken);
    }

    private async Task<T> RetryAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
    {
        Exception? last = null;
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                return await action();
            }
            catch (Exception exception) when (
                attempt < 3 &&
                exception is HttpRequestException or TaskCanceledException &&
                !cancellationToken.IsCancellationRequested)
            {
                last = exception;
                var delay = TimeSpan.FromSeconds(attempt * 2);
                logger.LogWarning(
                    exception,
                    "Transient Gemini embedding failure attempt {Attempt}, retrying in {Delay}.",
                    attempt,
                    delay);
                await Task.Delay(delay, cancellationToken);
            }
        }

        throw last ?? new InvalidOperationException("Gemini embedding failed.");
    }

    private static string NormalizeModel(string model) =>
        model.StartsWith("models/", StringComparison.OrdinalIgnoreCase) ? model[7..] : model;

    private static string ToApiTaskType(EmbeddingTaskType taskType) => taskType switch
    {
        EmbeddingTaskType.RetrievalDocument => "RETRIEVAL_DOCUMENT",
        EmbeddingTaskType.RetrievalQuery => "RETRIEVAL_QUERY",
        _ => "SEMANTIC_SIMILARITY"
    };

    private static void Normalize(float[] vector)
    {
        var magnitude = Math.Sqrt(vector.Sum(value => (double)value * value));
        if (magnitude <= 0)
            throw new InvalidOperationException("Gemini returned a zero-magnitude embedding.");

        for (var index = 0; index < vector.Length; index++)
            vector[index] = (float)(vector[index] / magnitude);
    }
}
