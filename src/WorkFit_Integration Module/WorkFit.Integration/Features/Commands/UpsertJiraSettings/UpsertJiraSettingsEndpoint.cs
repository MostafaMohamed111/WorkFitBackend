using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Integration.Contracts.IntegrationSyncService;
using WorkFit.Integration.Contracts.ProjectManagementProvider;
using WorkFit.Integration.Features.Shared;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Integration.Features.Commands.UpsertJiraSettings;

public sealed class UpsertJiraSettingsEndpoint : Endpoint<UpsertJiraSettingsRequest, JiraSettingsResponse>
{
    private readonly IMediator _mediator;

    public UpsertJiraSettingsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/integration/{OrganizationId}/jira-settings");
        Options(x => x.WithTags("Integration"));
    }

    public override async Task HandleAsync(UpsertJiraSettingsRequest req, CancellationToken ct)
    {
        if (req.OrganizationId == Guid.Empty)
            AddError(r => r.OrganizationId, "OrganizationId must not be empty.");
        if (string.IsNullOrWhiteSpace(req.BaseUrl))
            AddError(r => r.BaseUrl, "BaseUrl is required.");
        if (string.IsNullOrWhiteSpace(req.Email))
            AddError(r => r.Email, "Email is required.");
        if (string.IsNullOrWhiteSpace(req.ApiToken))
            AddError(r => r.ApiToken, "ApiToken is required.");
        if (string.IsNullOrWhiteSpace(req.ProjectKey))
            AddError(r => r.ProjectKey, "ProjectKey is required.");

        ThrowIfAnyErrors();

        var command = new UpsertJiraSettingsCommand(
            req.OrganizationId,
            req.BaseUrl,
            req.Email,
            req.ApiToken,
            req.ProjectKey,
            req.PageSize
        );

        var response = await _mediator.Send(command, ct);
        await Send.OkAsync(response, ct);
    }
}

