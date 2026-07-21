using WorkFit.Integration.Contracts.IntegrationSyncService;
using WorkFit.Integration.Contracts.ProjectManagementProvider;
using WorkFit.Integration.Features.Shared;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Integration.Features.Commands.UpsertJiraSettings;

internal sealed record UpsertJiraSettingsCommand(
    Guid OrganizationId,
    string BaseUrl,
    string Email,
    string ApiToken,
    string ProjectKey,
    int PageSize
) : IRequest<JiraSettingsResponse>;

