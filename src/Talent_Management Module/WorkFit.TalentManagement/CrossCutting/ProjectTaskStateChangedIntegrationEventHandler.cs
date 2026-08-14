using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Contracts.IntegrationEvents;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Domain.Entities;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.CrossCutting;

internal sealed class ProjectTaskStateChangedIntegrationEventHandler
    : IIntegrationEventHandler<ProjectTaskStateChangedIntegrationEvent>
{
    private readonly TalentDbContext _db;
    private readonly EmployeeIndexingStatePublisher _publisher;

    public ProjectTaskStateChangedIntegrationEventHandler(
        TalentDbContext db,
        EmployeeIndexingStatePublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task Handle(ProjectTaskStateChangedIntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var allocation = await _db.TaskAllocations
            .FirstOrDefaultAsync(x => x.TaskId == @event.TaskId, cancellationToken);

        if (allocation is not null && @event.Revision <= allocation.Revision)
            return;

        var previousEmployeeId = allocation?.EmployeeProfileId;
        if (allocation is null)
        {
            allocation = TaskAllocation.Create(
                @event.TaskId, @event.AssignedEmployeeId, @event.AllocationPercentage,
                @event.Status, @event.StoryPoints, @event.CompletedAt, @event.DeletedAt,
                @event.IsDeleted, @event.Revision, @event.OccurredAt);
            _db.TaskAllocations.Add(allocation);
        }
        else
        {
            allocation.Apply(
                @event.AssignedEmployeeId, @event.AllocationPercentage,
                @event.Status, @event.StoryPoints, @event.CompletedAt, @event.DeletedAt,
                @event.IsDeleted, @event.Revision, @event.OccurredAt);
        }

        var affectedEmployeeIds = new[] { previousEmployeeId, @event.AssignedEmployeeId }
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();

        var employees = await _db.EmployeeProfiles
            .Where(x => affectedEmployeeIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var employee in employees)
        {
            var otherAllocation = await _db.TaskAllocations
                .Where(x => x.EmployeeProfileId == employee.Id && x.TaskId != @event.TaskId)
                .Where(x => x.AllocationPercentage > 0 && !x.IsDeleted && x.DeletedAt == null &&
                    x.CompletedAt == null && x.Status != "Done")
                .SumAsync(x => x.AllocationPercentage, cancellationToken);
            var currentTaskAllocation = allocation.EmployeeProfileId == employee.Id && allocation.ContributesToAllocation
                ? allocation.AllocationPercentage
                : 0;

            employee.UpdateAllocation(otherAllocation + currentTaskAllocation);
        }

        await _db.SaveChangesAsync(cancellationToken);

        foreach (var employee in employees)
            await _publisher.PublishAsync(employee.Id, "TaskStateChanged", cancellationToken);
    }
}
