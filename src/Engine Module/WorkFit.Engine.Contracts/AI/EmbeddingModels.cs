namespace WorkFit.Engine.Contracts.AI;

public sealed record EmbeddingRequest(
    string Model,
    IReadOnlyList<string> Inputs,
    EmbeddingTaskType TaskType = EmbeddingTaskType.SemanticSimilarity,
    int? OutputDimensionality = null);

public enum EmbeddingTaskType
{
    SemanticSimilarity,
    RetrievalDocument,
    RetrievalQuery
}

public sealed record EmbeddingResponse(IReadOnlyList<ReadOnlyMemory<float>> Vectors);
