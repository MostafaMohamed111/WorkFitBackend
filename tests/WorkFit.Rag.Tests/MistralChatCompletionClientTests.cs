using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WorkFit.Engine.Contracts.AI;
using WorkFit.Engine.Infrastructure.AI;

namespace WorkFit.Rag.Tests;

public sealed class MistralChatCompletionClientTests
{
    [Fact]
    public async Task SendAsync_WithoutJsonResponseFormat_OmitsResponseFormat()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{\"choices\":[{\"message\":{\"content\":\"Hello\"},\"finish_reason\":\"stop\"}]}",
                Encoding.UTF8,
                "application/json")
        });
        var client = CreateClient(handler);

        var response = await client.SendAsync(new ChatCompletionRequest(
            string.Empty,
            [new ChatMessage("user", "Hello")]));

        Assert.Equal("Hello", response.Content);
        using var payload = JsonDocument.Parse(handler.RequestBodies.Single());
        Assert.False(payload.RootElement.TryGetProperty("response_format", out _));
        Assert.False(payload.RootElement.TryGetProperty("max_tokens", out _));
    }

    [Fact]
    public async Task SendAsync_UnprocessableEntity_DoesNotRetry()
    {
        var handler = new RecordingHandler(_ => new HttpResponseMessage(HttpStatusCode.UnprocessableEntity)
        {
            Content = new StringContent("{\"message\":\"invalid request\"}")
        });
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => client.SendAsync(
            new ChatCompletionRequest(string.Empty, [new ChatMessage("user", "Hello")])));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, exception.StatusCode);
        Assert.Single(handler.RequestBodies);
    }

    private static MistralChatCompletionClient CreateClient(RecordingHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.mistral.ai/") };
        var options = new AIOptions
        {
            Providers = new Dictionary<string, AIProviderOptions>
            {
                ["Mistral"] = new("Mistral", "mistral-small-latest", "test-key", "https://api.mistral.ai/")
            }
        };

        return new MistralChatCompletionClient(
            new TestHttpClientFactory(httpClient),
            Options.Create(options),
            NullLogger<MistralChatCompletionClient>.Instance);
    }

    private sealed class TestHttpClientFactory(HttpClient client) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => client;
    }

    private sealed class RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        public List<string> RequestBodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestBodies.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            return responseFactory(request);
        }
    }
}
