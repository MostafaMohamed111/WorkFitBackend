using WorkFit.Integration.Contracts.IntegrationSyncService;
using WorkFit.Integration.Contracts.ProjectManagementProvider;
using WorkFit.Integration.Features.Shared;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Integration.Features.Queries.GetJiraSettings;

internal sealed record GetJiraSettingsQuery(Guid OrganizationId) : IRequest<JiraSettingsResponse>;

