using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Integration.Contracts.IntegrationSyncService;
using WorkFit.Integration.Contracts.ProjectManagementProvider;
using WorkFit.Integration.Features.Shared;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.Organizations.Contracts.OrganizationServices;

namespace WorkFit.Integration.Features.Commands.SyncIntegration;

public sealed class SyncIntegrationEndpoint : Endpoint<SyncIntegrationRequest, SyncResult>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUser;
    private readonly IGetOrganizationIdService _organizations;

    public SyncIntegrationEndpoint(IMediator mediator, ICurrentUserContext currentUser, IGetOrganizationIdService organizations)
    {
        _mediator = mediator; _currentUser = currentUser; _organizations = organizations;
    }

    public override void Configure()
    {
        Post("/api/integration/sync");
        Roles("TeamLeader", "OrganizationOwner");
        Options(x => x.WithTags("Integration"));
    }

    public override async Task HandleAsync(SyncIntegrationRequest req, CancellationToken ct)
    {
        if (req.OrganizationId == Guid.Empty)
            AddError(r => r.OrganizationId, "OrganizationId must not be empty.");

        ThrowIfAnyErrors();

        var callerOrganizationId = await _organizations.GetOrganizationIdAsync(_currentUser.GetUserId(ct), ct);
        if (callerOrganizationId != req.OrganizationId)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        var command = new SyncIntegrationCommand(
            req.OrganizationId
        );

        var result = await _mediator.Send(command, ct);
        await Send.OkAsync(result, ct);
    }
}

