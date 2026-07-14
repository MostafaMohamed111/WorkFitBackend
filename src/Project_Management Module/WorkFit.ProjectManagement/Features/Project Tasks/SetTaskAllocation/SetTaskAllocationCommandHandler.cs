using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Contracts.IntegrationEvents;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.SetTaskAllocation;
public sealed class SetTaskAllocationCommandHandler : IRequestHandler<SetTaskAllocationCommand, SetTaskAllocationResponse>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly IMediator _mediator;

    public SetTaskAllocationCommandHandler(WorkFitProjectDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<SetTaskAllocationResponse> Handle(SetTaskAllocationCommand command, CancellationToken ct)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == command.TaskId, ct)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);

        if (task.AssigneeId is null)
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "TASK_HAS_NO_ASSIGNEE",
                "Cannot set allocation for a task with no assignee.",
                "This task must have an assignee before setting an allocation percentage.");

        task.SetAllocationPercentage(command.AllocationPercentage);
        await _context.SaveChangesAsync(ct);

        if (task.IsActive)
        {
            await _mediator.Publish(new TaskAssignedIntegrationEvent(
                task.Id, task.AssigneeId!.Value, task.AllocationPercentage), ct);
        }

        return new SetTaskAllocationResponse(task.Id, task.AllocationPercentage, task.IsActive);
    }
}