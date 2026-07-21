using WorkFit.Integration.Contracts.IntegrationSyncService;
using WorkFit.Integration.Contracts.ProjectManagementProvider;
using WorkFit.Integration.Features.Shared;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Integration.Features.Commands.SyncIntegration;

internal sealed record SyncIntegrationCommand(
    Guid OrganizationId
) : IRequest<SyncResult>;

