using Microsoft.EntityFrameworkCore;
using WorkFit.Integration.Contracts.IntegrationSyncService;
using WorkFit.Integration.Contracts.ProjectManagementProvider;
using WorkFit.Integration.Features.Shared;
using WorkFit.Integration.Domain.Entities;
using WorkFit.Integration.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Integration.Features.Commands.UpsertJiraSettings;

internal sealed class UpsertJiraSettingsCommandHandler : IRequestHandler<UpsertJiraSettingsCommand, JiraSettingsResponse>
{
    private readonly IntegrationDbContext _db;

    public UpsertJiraSettingsCommandHandler(IntegrationDbContext db) => _db = db;

    public async Task<JiraSettingsResponse> Handle(UpsertJiraSettingsCommand request, CancellationToken cancellationToken = default)
    {
        var existing = await _db.OrganizationIntegrationSettings
            .FirstOrDefaultAsync(
                s => s.OrganizationId == request.OrganizationId && s.Provider == "Jira", cancellationToken);

        if (existing is not null)
        {
            existing.Update(request.BaseUrl, request.Email, request.ApiToken, request.ProjectKey, request.PageSize);
        }
        else
        {
            existing = OrganizationIntegrationSetting.Create(
                request.OrganizationId,
                "Jira",
                request.BaseUrl,
                request.Email,
                request.ApiToken,
                request.ProjectKey,
                request.PageSize);

            await _db.OrganizationIntegrationSettings.AddAsync(existing, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return new JiraSettingsResponse(
            existing.Id,
            existing.OrganizationId,
            existing.BaseUrl,
            existing.Email,
            MaskToken(existing.ApiToken),
            existing.ProjectKey,
            existing.PageSize,
            existing.CreatedAt,
            existing.UpdatedAt);
    }

    private static string MaskToken(string token)
    {
        if (string.IsNullOrEmpty(token) || token.Length <= 4)
            return "****";
        return new string('*', token.Length - 4) + token[^4..];
    }
}

