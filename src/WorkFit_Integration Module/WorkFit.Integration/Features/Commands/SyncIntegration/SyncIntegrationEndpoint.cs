using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WorkFit.Integration.Contracts.IntegrationSyncService;
using WorkFit.Integration.Contracts.ProjectManagementProvider;
using WorkFit.Integration.Features.Shared;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.ProjectManagement.Contracts.Membership;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;

namespace WorkFit.Integration.Features.Commands.SyncIntegration;

public sealed class SyncIntegrationEndpoint : Endpoint<SyncIntegrationRequest, SyncResult>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUser;
    private readonly IGetOrganizationIdService _organizations;
    private readonly IProjectMembershipService _projects;
    private readonly ILogger<SyncIntegrationEndpoint> _logger;

    public SyncIntegrationEndpoint(
        IMediator mediator,
        ICurrentUserContext currentUser,
        IGetOrganizationIdService organizations,
        IProjectMembershipService projects,
        ILogger<SyncIntegrationEndpoint> logger)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _organizations = organizations;
        _projects = projects;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/integration/sync");
        Roles("TeamLeader", "OrganizationOwner", "Admin", "SuperAdmin");
        Options(x => x.WithTags("Integration"));
    }

    public override async Task HandleAsync(SyncIntegrationRequest req, CancellationToken ct)
    {
        if (req.OrganizationId == Guid.Empty)
            AddError(r => r.OrganizationId, "OrganizationId must not be empty.");

        ThrowIfAnyErrors();

        var userId = _currentUser.GetUserId(ct);
        var roles = _currentUser.GetRoles(ct);
        var isManagementOrLead = roles.Any(r => r is "TeamLeader" or "OrganizationOwner" or "Admin" or "SuperAdmin");

        if (!isManagementOrLead)
        {
            _logger.LogWarning(
                "Jira sync authorization rejected for user {UserId}: missing management or lead role",
                userId);
            await Send.ForbiddenAsync(ct);
            return;
        }

        var authorized = false;
        if (roles.Contains("SuperAdmin") || roles.Contains("Admin"))
        {
            authorized = true;
        }
        else
        {
            try
            {
                var userOrgId = await _organizations.GetOrganizationIdAsync(userId, ct);
                authorized = userOrgId == req.OrganizationId;
            }
            catch
            {
                authorized = isManagementOrLead;
            }

            if (!authorized && roles.Contains("TeamLeader"))
            {
                authorized = await _projects.IsTeamLeaderInOrganizationAsync(userId, req.OrganizationId, ct);
            }
        }

        if (!authorized)
        {
            _logger.LogWarning(
                "Jira sync authorization rejected for user {UserId} and organization {OrganizationId}",
                userId,
                req.OrganizationId);
            await Send.ForbiddenAsync(ct);
            return;
        }

        _logger.LogInformation(
            "Jira sync request accepted for user {UserId} and organization {OrganizationId}",
            userId,
            req.OrganizationId);

        var command = new SyncIntegrationCommand(
            req.OrganizationId
        );

        var result = await _mediator.Send(command, ct);
        await Send.OkAsync(result, ct);
    }
}

