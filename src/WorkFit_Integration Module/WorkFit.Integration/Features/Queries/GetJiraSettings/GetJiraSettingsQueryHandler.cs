using Microsoft.EntityFrameworkCore;
using WorkFit.Integration.Contracts.ProjectManagementProvider;
using WorkFit.Integration.Features.Shared;
using WorkFit.Integration.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Integration.Features.Queries.GetJiraSettings;

internal sealed class GetJiraSettingsQueryHandler : IRequestHandler<GetJiraSettingsQuery, JiraSettingsResponse>
{
    private readonly IntegrationDbContext _db;

    public GetJiraSettingsQueryHandler(IntegrationDbContext db) => _db = db;

    public async Task<JiraSettingsResponse> Handle(GetJiraSettingsQuery request, CancellationToken cancellationToken = default)
    {
        var setting = await _db.OrganizationIntegrationSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.OrganizationId == request.OrganizationId && s.Provider == "Jira", cancellationToken);

        if (setting is null)
        {
            // Return 200 OK with empty settings response if Jira is not yet configured for this organization
            return new JiraSettingsResponse(
                Guid.Empty,
                request.OrganizationId,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                100,
                DateTime.UtcNow,
                null);
        }

        return new JiraSettingsResponse(
            setting.Id,
            setting.OrganizationId,
            setting.BaseUrl,
            setting.Email,
            MaskToken(setting.ApiToken),
            setting.ProjectKey,
            setting.PageSize,
            setting.CreatedAt,
            setting.UpdatedAt);
    }

    private static string MaskToken(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length <= 4)
            return "****";
        return new string('*', token.Length - 4) + token[^4..];
    }
}
