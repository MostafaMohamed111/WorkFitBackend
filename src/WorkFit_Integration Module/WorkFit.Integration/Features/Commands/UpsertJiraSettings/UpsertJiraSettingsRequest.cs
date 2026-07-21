using FastEndpoints;

namespace WorkFit.Integration.Features.Commands.UpsertJiraSettings;

public sealed record UpsertJiraSettingsRequest
{
    [BindFrom("organizationId")]
    public Guid OrganizationId { get; init; }

    public string BaseUrl { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string ApiToken { get; init; } = null!;
    public string ProjectKey { get; init; } = null!;
    public int PageSize { get; init; } = 100;
}
