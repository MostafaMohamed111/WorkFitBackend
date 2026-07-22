using FastEndpoints;

namespace WorkFit.Integration.Features.Queries.GetJiraSettings;

public sealed record GetJiraSettingsRequest
{
    [BindFrom("organizationId")]
    public Guid OrganizationId { get; init; }
}
