namespace WorkFit.Engine.Contracts.AI;

public interface IEmbeddingClient
{
    Task<EmbeddingResponse> EmbedAsync(EmbeddingRequest request, CancellationToken cancellationToken = default);
}
