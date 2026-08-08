namespace WorkFit.Engine.Contracts.AI;

public interface IChatCompletionClient
{
    Task<ChatCompletionResponse> SendAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default);
}
