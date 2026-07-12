using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Integration.Infrastructure.Data;

namespace WorkFit.Integration.Features.JiraSettings;

// ── Request ────────────────────────────────────────────────────────────────────
public sealed record DeleteJiraSettingsRequest
{
    [BindFrom("organizationId")]
    public Guid OrganizationId { get; init; }
}

// ── Endpoint ───────────────────────────────────────────────────────────────────

/// <summary>
/// DELETE /api/integration/{OrganizationId}/jira-settings
///
/// Removes the Jira integration settings for the given organization.
/// </summary>
public sealed class DeleteJiraSettingsEndpoint : Endpoint<DeleteJiraSettingsRequest>
{
    private readonly IntegrationDbContext _db;

    public DeleteJiraSettingsEndpoint(IntegrationDbContext db) => _db = db;

    public override void Configure()
    {
        Delete("/api/integration/{OrganizationId}/jira-settings");
        AllowAnonymous(); // TODO: restrict to Admin / OrganizationOwner
        Options(x => x.WithTags("Integration"));
    }

    public override async Task HandleAsync(DeleteJiraSettingsRequest req, CancellationToken ct)
    {
        if (req.OrganizationId == Guid.Empty)
        {
            AddError(r => r.OrganizationId, "OrganizationId must not be empty.");
            ThrowIfAnyErrors();
        }

        var setting = await _db.OrganizationIntegrationSettings
            .FirstOrDefaultAsync(
                s => s.OrganizationId == req.OrganizationId && s.Provider == "Jira", ct);

        if (setting is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        _db.OrganizationIntegrationSettings.Remove(setting);
        await _db.SaveChangesAsync(ct);

        await Send.NoContentAsync(ct);
    }
}
