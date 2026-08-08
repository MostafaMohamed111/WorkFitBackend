namespace WorkFit.Engine.Contracts.AI;

public sealed record ChatCompletionRequest(
    string Model,
    IReadOnlyList<ChatMessage> Messages,
    double Temperature = 0.2,
    bool? ResponseFormatJson = null,
    int? MaxTokens = null);

public sealed record ChatMessage(string Role, string Content);

public sealed record ChatCompletionResponse(
    string Content,
    string? FinishReason,
    int? PromptTokens,
    int? CompletionTokens);
