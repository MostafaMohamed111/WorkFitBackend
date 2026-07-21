using Microsoft.EntityFrameworkCore;
using WorkFit.Integration.Contracts.IntegrationSyncService;
using WorkFit.Integration.Contracts.ProjectManagementProvider;
using WorkFit.Integration.Features.Shared;
using WorkFit.Integration.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.Integration.Domain.Entities;

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
            throw new EntityNotFoundException(ModuleMarker.ModuleName, typeof(OrganizationIntegrationSetting).Name, request.OrganizationId);
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

