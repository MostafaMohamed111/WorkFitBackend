using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Integration.Contracts.Dtos;
using WorkFit.Integration.Domain.Entities;
using WorkFit.Integration.Infrastructure.Data;

namespace WorkFit.Integration.Features.JiraSettings;

// ── Request ────────────────────────────────────────────────────────────────────
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

// ── Endpoint ───────────────────────────────────────────────────────────────────

/// <summary>
/// PUT /api/integration/{OrganizationId}/jira-settings
///
/// Creates or updates the Jira integration settings for the given organization.
/// Each organization may have at most one Jira configuration.
/// </summary>
public sealed class UpsertJiraSettingsEndpoint : Endpoint<UpsertJiraSettingsRequest, JiraSettingsResponse>
{
    private readonly IntegrationDbContext _db;

    public UpsertJiraSettingsEndpoint(IntegrationDbContext db) => _db = db;

    public override void Configure()
    {
        Put("/api/integration/{OrganizationId}/jira-settings");
        AllowAnonymous(); // TODO: restrict to Admin / OrganizationOwner
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

        var existing = await _db.OrganizationIntegrationSettings
            .FirstOrDefaultAsync(
                s => s.OrganizationId == req.OrganizationId && s.Provider == "Jira", ct);

        if (existing is not null)
        {
            existing.Update(req.BaseUrl, req.Email, req.ApiToken, req.ProjectKey, req.PageSize);
        }
        else
        {
            existing = OrganizationIntegrationSetting.Create(
                req.OrganizationId,
                "Jira",
                req.BaseUrl,
                req.Email,
                req.ApiToken,
                req.ProjectKey,
                req.PageSize);

            await _db.OrganizationIntegrationSettings.AddAsync(existing, ct);
        }

        await _db.SaveChangesAsync(ct);

        var response = new JiraSettingsResponse(
            existing.Id,
            existing.OrganizationId,
            existing.BaseUrl,
            existing.Email,
            MaskToken(existing.ApiToken),
            existing.ProjectKey,
            existing.PageSize,
            existing.CreatedAt,
            existing.UpdatedAt);

        await Send.OkAsync(response, ct);
    }

    private static string MaskToken(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length <= 4)
            return "****";
        return new string('*', token.Length - 4) + token[^4..];
    }
}
