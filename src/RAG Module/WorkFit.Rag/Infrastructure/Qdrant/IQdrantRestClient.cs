namespace WorkFit.Rag.Infrastructure.Qdrant;

internal interface IQdrantRestClient
{
    Task EnsureCollectionsAsync(CancellationToken cancellationToken);

    Task UpsertAsync(string collection, QdrantPoint point, CancellationToken cancellationToken);

    Task DeleteAsync(string collection, Guid pointId, CancellationToken cancellationToken);

    Task<IReadOnlyList<QdrantSearchResult>> SearchAsync(
        string collection,
        ReadOnlyMemory<float> vector,
        int limit,
        QdrantFilter? filter,
        CancellationToken cancellationToken);
}
