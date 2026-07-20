//using FastEndpoints;
//using Microsoft.AspNetCore.Http;
//using Microsoft.EntityFrameworkCore;
//using WorkFit.Integration.Contracts.Abstractions;
//using WorkFit.Integration.Contracts.Dtos;
//using WorkFit.Integration.Domain.Entities;
//using WorkFit.Integration.Infrastructure.Data;

//namespace WorkFit.Integration.Features.SyncIntegration;

///// <summary>
///// POST /api/integration/sync
/////
///// Saves (or updates) the Jira settings for the organization, then triggers
///// a full sync from Jira into the WorkFit database.
///// One call does both — no separate PUT required.
///// </summary>
//public sealed class SyncIntegrationEndpoint : Endpoint<SyncIntegrationRequest, SyncResult>
//{
//    private readonly IIntegrationSyncService _syncService;
//    private readonly IntegrationDbContext _integrationDb;

//    public SyncIntegrationEndpoint(
//        IIntegrationSyncService syncService,
//        IntegrationDbContext integrationDb)
//    {
//        _syncService = syncService;
//        _integrationDb = integrationDb;
//    }

//    public override void Configure()
//    {
//        Post("/api/integration/sync");
//        AllowAnonymous(); // TODO: restrict to Admin / SuperAdmin
//        Options(x => x.WithTags("Integration"));
//    }

//    public override async Task HandleAsync(SyncIntegrationRequest req, CancellationToken ct)
//    {
//        if (req.OrganizationId == Guid.Empty)
//            AddError(r => r.OrganizationId, "OrganizationId must not be empty.");

//        if (req.DepartmentId == Guid.Empty)
//            AddError(r => r.DepartmentId, "DepartmentId must not be empty.");

//        if (string.IsNullOrWhiteSpace(req.BaseUrl))
//            AddError(r => r.BaseUrl, "BaseUrl is required.");

//        if (string.IsNullOrWhiteSpace(req.Email))
//            AddError(r => r.Email, "Email is required.");

//        if (string.IsNullOrWhiteSpace(req.ApiToken))
//            AddError(r => r.ApiToken, "ApiToken is required.");

//        if (string.IsNullOrWhiteSpace(req.ProjectKey))
//            AddError(r => r.ProjectKey, "ProjectKey is required.");

//        ThrowIfAnyErrors();

//        // ── Save / update the Jira settings for this organization ────────────
//        var existing = await _integrationDb.OrganizationIntegrationSettings
//            .FirstOrDefaultAsync(
//                s => s.OrganizationId == req.OrganizationId && s.Provider == "Jira", ct);

//        if (existing is not null)
//        {
//            existing.Update(req.BaseUrl, req.Email, req.ApiToken, req.ProjectKey, req.PageSize);
//        }
//        else
//        {
//            existing = OrganizationIntegrationSetting.Create(
//                req.OrganizationId,
//                "Jira",
//                req.BaseUrl,
//                req.Email,
//                req.ApiToken,
//                req.ProjectKey,
//                req.PageSize);

//            await _integrationDb.OrganizationIntegrationSettings.AddAsync(existing, ct);
//        }

//        await _integrationDb.SaveChangesAsync(ct);

//        // ── Now trigger the sync (provider will load settings from DB) ────────
//        var result = await _syncService.SyncAsync(req.OrganizationId, req.DepartmentId, ct);
//        await Send.OkAsync(result, ct);
//    }
//}
