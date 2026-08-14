using Microsoft.Extensions.Logging;
using WorkFit.Rag.Contracts.Indexing;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Contracts.IntegrationEvents;

namespace WorkFit.Rag.CrossCutting;

internal sealed class EmployeeIndexingStateChangedIntegrationEventHandler(
    IEmployeeProfileIndexingService indexingService,
    ILogger<EmployeeIndexingStateChangedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<EmployeeIndexingStateChangedIntegrationEvent>
{
    public async Task Handle(
        EmployeeIndexingStateChangedIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(@event.Employee);

        var employee = @event.Employee;
        try
        {
            if (EmployeeIndexDocumentMapper.IsRemoved(employee, @event.ChangeType))
                await indexingService.DeleteAsync(employee.EmployeeProfileId, cancellationToken);
            else
                await indexingService.UpsertAsync(EmployeeIndexDocumentMapper.Map(employee), cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Employee RAG indexing failed for employee {EmployeeProfileId} at revision {Revision}; lazy hydration will retry.",
                employee.EmployeeProfileId,
                employee.SnapshotAt);
        }
    }
}
