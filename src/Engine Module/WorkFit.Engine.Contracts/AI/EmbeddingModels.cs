namespace WorkFit.Engine.Contracts.AI;

public sealed record EmbeddingRequest(string Model, IReadOnlyList<string> Inputs);

public sealed record EmbeddingResponse(IReadOnlyList<ReadOnlyMemory<float>> Vectors);
