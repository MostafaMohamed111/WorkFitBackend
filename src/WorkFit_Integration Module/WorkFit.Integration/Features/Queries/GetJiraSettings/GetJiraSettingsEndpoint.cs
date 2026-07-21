using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Integration.Contracts.IntegrationSyncService;
using WorkFit.Integration.Contracts.ProjectManagementProvider;
using WorkFit.Integration.Features.Shared;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Integration.Features.Queries.GetJiraSettings;

public sealed class GetJiraSettingsEndpoint : Endpoint<GetJiraSettingsRequest, JiraSettingsResponse>
{
    private readonly IMediator _mediator;

    public GetJiraSettingsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/integration/{OrganizationId}/jira-settings");
        Roles("Admin"); // Applied security role
        Options(x => x.WithTags("Integration"));
    }

    public override async Task HandleAsync(GetJiraSettingsRequest req, CancellationToken ct)
    {
        if (req.OrganizationId == Guid.Empty)
        {
            AddError(r => r.OrganizationId, "OrganizationId must not be empty.");
            ThrowIfAnyErrors();
        }

        var response = await _mediator.Send(new GetJiraSettingsQuery(req.OrganizationId), ct);
        await Send.OkAsync(response, ct);
    }
}

