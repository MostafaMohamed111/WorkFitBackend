namespace WorkFit.CodeReview.Infrastructure.Options;

public sealed class CodeReviewOptions
{
    public GitHubOptions GitHub { get; init; } = new();
    public MistralOptions Mistral { get; init; } = new();
    public TimeSpan MetadataCacheTtl { get; init; } = TimeSpan.FromHours(24);

    public sealed class GitHubOptions
    {
        public string BaseUrl { get; init; } = "https://api.github.com/";
        public string? PersonalAccessToken { get; init; }
        public string? AppId { get; init; }
        public string? AppPrivateKey { get; init; }
        public string? WebhookSecret { get; init; }
        public string ApiVersion { get; init; } = "2022-11-28";
        public string UserAgent { get; init; } = "WorkFit.CodeReview";
    }

    public sealed class MistralOptions
    {
        public string BaseUrl { get; init; } = "https://api.mistral.ai/v1/chat/completions";
        public string? ApiKey { get; init; }
        public string Model { get; init; } = "mistral-small-2603";
        public string UserAgent { get; init; } = "WorkFit.CodeReview";
    }
}
