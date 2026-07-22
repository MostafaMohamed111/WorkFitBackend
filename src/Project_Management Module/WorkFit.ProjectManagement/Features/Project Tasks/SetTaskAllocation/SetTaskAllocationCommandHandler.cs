using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Contracts.IntegrationEvents;
using WorkFit.ProjectManagement.Features.Exceptions;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.SetTaskAllocation;
public sealed class SetTaskAllocationCommandHandler : IRequestHandler<SetTaskAllocationCommand, Guid>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUser;

    public SetTaskAllocationCommandHandler(WorkFitProjectDbContext context, IMediator mediator, ICurrentUserContext currentUser)
    {
        _context = context;
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(SetTaskAllocationCommand command, CancellationToken ct)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == command.TaskId, ct)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);

        var actorId = _currentUser.GetUserId(ct);

        if (task.AssignedEmployeeId is null)
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "TASK_HAS_NO_ASSIGNEE",
                "Cannot set allocation for a task with no assignee.",
                "This task must have an assignee before setting an allocation percentage.");


        if(actorId != task.CreatedById)
            throw new UnAuthorizedTeamLeadAccessException(actorId);

        task.SetAllocationPercentage(command.AllocationPercentage);
        await _context.SaveChangesAsync(ct);

        if (task.IsActive)
        {
            await _mediator.Publish(new TaskAssignedIntegrationEvent(
                task.Id, task.AssignedEmployeeId!.Value, task.AllocationPercentage), ct);
        }

        return task.Id;
    }
}