using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Contracts.Indexing;
using WorkFit.TalentManagement.Contracts.IntegrationEvents;

namespace WorkFit.TalentManagement.CrossCutting;

public sealed class EmployeeIndexingStatePublisher
{
    private readonly IEmployeeIndexingSnapshotService _snapshots;
    private readonly IMediator _mediator;

    public EmployeeIndexingStatePublisher(IEmployeeIndexingSnapshotService snapshots, IMediator mediator)
    {
        _snapshots = snapshots;
        _mediator = mediator;
    }

    public async Task PublishAsync(Guid employeeProfileId, string changeType, CancellationToken cancellationToken)
    {
        var snapshot = await _snapshots.GetEmployeeAsync(employeeProfileId, cancellationToken);
        if (snapshot is null)
            return;

        await _mediator.Publish(
            new EmployeeIndexingStateChangedIntegrationEvent(snapshot, changeType, DateTimeOffset.UtcNow),
            cancellationToken);
    }
}
