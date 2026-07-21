using FastEndpoints;

namespace WorkFit.Integration.Features.Commands.DeleteJiraSettings;

public sealed record DeleteJiraSettingsRequest
{
    [BindFrom("organizationId")]
    public Guid OrganizationId { get; init; }
}
