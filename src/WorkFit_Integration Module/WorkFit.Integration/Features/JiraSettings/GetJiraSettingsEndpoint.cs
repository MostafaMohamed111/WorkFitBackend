using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Integration.Contracts.Dtos;
using WorkFit.Integration.Infrastructure.Data;

namespace WorkFit.Integration.Features.JiraSettings;

// ── Request ────────────────────────────────────────────────────────────────────
public sealed record GetJiraSettingsRequest
{
    [BindFrom("organizationId")]
    public Guid OrganizationId { get; init; }
}

// ── Endpoint ───────────────────────────────────────────────────────────────────

/// <summary>
/// GET /api/integration/{OrganizationId}/jira-settings
///
/// Returns the Jira integration settings for the given organization.
/// The API token is masked in the response.
/// </summary>
public sealed class GetJiraSettingsEndpoint : Endpoint<GetJiraSettingsRequest, JiraSettingsResponse>
{
    private readonly IntegrationDbContext _db;

    public GetJiraSettingsEndpoint(IntegrationDbContext db) => _db = db;

    public override void Configure()
    {
        Get("/api/integration/{OrganizationId}/jira-settings");
        AllowAnonymous(); // TODO: restrict to Admin / OrganizationOwner
        Options(x => x.WithTags("Integration"));
    }

    public override async Task HandleAsync(GetJiraSettingsRequest req, CancellationToken ct)
    {
        if (req.OrganizationId == Guid.Empty)
        {
            AddError(r => r.OrganizationId, "OrganizationId must not be empty.");
            ThrowIfAnyErrors();
        }

        var setting = await _db.OrganizationIntegrationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.OrganizationId == req.OrganizationId && s.Provider == "Jira", ct);

        if (setting is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = new JiraSettingsResponse(
            setting.Id,
            setting.OrganizationId,
            setting.BaseUrl,
            setting.Email,
            MaskToken(setting.ApiToken),
            setting.ProjectKey,
            setting.PageSize,
            setting.CreatedAt,
            setting.UpdatedAt);

        await Send.OkAsync(response, ct);
    }

    private static string MaskToken(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length <= 4)
            return "****";
        return new string('*', token.Length - 4) + token[^4..];
    }
}
