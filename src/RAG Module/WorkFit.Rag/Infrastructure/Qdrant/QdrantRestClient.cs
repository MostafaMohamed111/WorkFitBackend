using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WorkFit.Rag.Infrastructure.Options;

namespace WorkFit.Rag.Infrastructure.Qdrant;

internal sealed class QdrantRestClient(
    IHttpClientFactory httpClientFactory,
    IOptions<QdrantOptions> options) : IQdrantRestClient
{
    private readonly QdrantOptions _options = options.Value;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private volatile bool _collectionsInitialized;

    public async Task EnsureCollectionsAsync(CancellationToken cancellationToken)
    {
        if (_collectionsInitialized)
        {
            return;
        }

        await _initializationLock.WaitAsync(cancellationToken);
        try
        {
            if (_collectionsInitialized)
            {
                return;
            }

            await EnsureCollectionAsync(_options.EmployeeProfilesCollection, cancellationToken);
            await EnsureCollectionAsync(_options.ProjectTasksCollection, cancellationToken);
            _collectionsInitialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task UpsertAsync(string collection, QdrantPoint point, CancellationToken cancellationToken)
    {
        ValidateVector(point.Vector);
        await EnsureCollectionsAsync(cancellationToken);

        var body = new
        {
            points = new[]
            {
                new { id = point.Id, vector = point.Vector.ToArray(), payload = point.Payload }
            }
        };
        using var response = await Client().PutAsJsonAsync(
            $"collections/{Uri.EscapeDataString(collection)}/points?wait=true", body, cancellationToken);
        await EnsureSuccessAsync(response, "upsert point", cancellationToken);
    }

    public async Task DeleteAsync(string collection, Guid pointId, CancellationToken cancellationToken)
    {
        await EnsureCollectionsAsync(cancellationToken);
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"collections/{Uri.EscapeDataString(collection)}/points/delete?wait=true")
        {
            Content = JsonContent.Create(new { points = new[] { pointId } })
        };
        using var response = await Client().SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, "delete point", cancellationToken);
    }

    public async Task<IReadOnlyList<QdrantSearchResult>> SearchAsync(
        string collection,
        ReadOnlyMemory<float> vector,
        int limit,
        QdrantFilter? filter,
        CancellationToken cancellationToken)
    {
        ValidateVector(vector);
        await EnsureCollectionsAsync(cancellationToken);
        var body = new
        {
            vector = vector.ToArray(),
            limit = Math.Clamp(limit, 1, 100),
            with_payload = true,
            filter = filter is null ? null : new
            {
                must = filter.Must.Select(condition => new
                {
                    key = condition.Key,
                    match = condition.Match,
                    range = condition.Range
                })
            }
        };

        using var response = await Client().PostAsJsonAsync(
            $"collections/{Uri.EscapeDataString(collection)}/points/search", body, cancellationToken);
        await EnsureSuccessAsync(response, "search points", cancellationToken);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        if (!document.RootElement.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Qdrant search returned an invalid result payload.");
        }

        var points = new List<QdrantSearchResult>();
        foreach (var item in result.EnumerateArray())
        {
            if (!item.TryGetProperty("id", out var idElement) ||
                !Guid.TryParse(idElement.ToString(), out var id) ||
                !item.TryGetProperty("score", out var scoreElement) ||
                !item.TryGetProperty("payload", out var payload))
            {
                continue;
            }

            points.Add(new QdrantSearchResult(id, scoreElement.GetDouble(), payload.Clone()));
        }

        return points;
    }

    private async Task EnsureCollectionAsync(string collection, CancellationToken cancellationToken)
    {
        using var getResponse = await Client().GetAsync(
            $"collections/{Uri.EscapeDataString(collection)}", cancellationToken);
        if (getResponse.IsSuccessStatusCode)
        {
            await ValidateCollectionAsync(getResponse, collection, cancellationToken);
            return;
        }

        if (getResponse.StatusCode != HttpStatusCode.NotFound)
        {
            await EnsureSuccessAsync(getResponse, "inspect collection", cancellationToken);
        }

        using var createResponse = await Client().PutAsJsonAsync(
            $"collections/{Uri.EscapeDataString(collection)}",
            new { vectors = new { size = QdrantOptions.VectorSize, distance = "Cosine" } },
            cancellationToken);
        if (createResponse.StatusCode != HttpStatusCode.Conflict)
        {
            await EnsureSuccessAsync(createResponse, "create collection", cancellationToken);
        }
    }

    private static async Task ValidateCollectionAsync(
        HttpResponseMessage response,
        string collection,
        CancellationToken cancellationToken)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        if (!document.RootElement.TryGetProperty("result", out var result) ||
            !result.TryGetProperty("config", out var config) ||
            !config.TryGetProperty("params", out var parameters) ||
            !parameters.TryGetProperty("vectors", out var vectors) ||
            !vectors.TryGetProperty("size", out var size) ||
            !vectors.TryGetProperty("distance", out var distance) ||
            size.GetInt32() != QdrantOptions.VectorSize ||
            !string.Equals(distance.GetString(), "Cosine", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Qdrant collection '{collection}' must use {QdrantOptions.VectorSize}-dimensional cosine vectors.");
        }
    }

    private HttpClient Client() => httpClientFactory.CreateClient("RagQdrant");

    private static void ValidateVector(ReadOnlyMemory<float> vector)
    {
        if (vector.Length != QdrantOptions.VectorSize)
        {
            throw new InvalidOperationException(
                $"Embedding dimension must be {QdrantOptions.VectorSize}, but was {vector.Length}.");
        }
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operation,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(
            $"Qdrant failed to {operation} ({(int)response.StatusCode}): {content}",
            null,
            response.StatusCode);
    }
}
