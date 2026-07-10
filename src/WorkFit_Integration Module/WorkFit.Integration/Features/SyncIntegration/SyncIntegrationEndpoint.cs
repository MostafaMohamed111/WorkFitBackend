using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Integration.Contracts.Abstractions;
using WorkFit.Integration.Contracts.Dtos;

namespace WorkFit.Integration.Features.SyncIntegration;

/// <summary>
/// POST /api/integration/sync
///
/// Triggers a full sync from the configured external PM provider (currently Jira)
/// into the WorkFit database.
/// </summary>
public sealed class SyncIntegrationEndpoint : Endpoint<SyncIntegrationRequest, SyncResult>
{
    private readonly IIntegrationSyncService _syncService;

    public SyncIntegrationEndpoint(IIntegrationSyncService syncService)
    {
        _syncService = syncService;
    }

    public override void Configure()
    {
        Post("/api/integration/sync");
        AllowAnonymous(); // TODO: restrict to Admin / SuperAdmin
        Options(x => x.WithTags("Integration"));
    }

    public override async Task HandleAsync(SyncIntegrationRequest req, CancellationToken ct)
    {
        if (req.OrganizationId == Guid.Empty)
            AddError(r => r.OrganizationId, "OrganizationId must not be empty.");

        if (req.DepartmentId == Guid.Empty)
            AddError(r => r.DepartmentId, "DepartmentId must not be empty.");

        ThrowIfAnyErrors();

        var result = await _syncService.SyncAsync(req.OrganizationId, req.DepartmentId, ct);
        await Send.OkAsync(result, ct);
    }
}
