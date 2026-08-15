using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Contracts.IntegrationEvents;
using WorkFit.ProjectManagement.Features.Exceptions;
using WorkFit.ProjectManagement.CrossCutting;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.ProjectManagement.Features.Common;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.AssignTask;

public sealed class AssignTaskCommandHandler : IRequestHandler<AssignTaskCommand, Guid>
{
    private readonly WorkFitProjectDbContext _context;

    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUser;

    public AssignTaskCommandHandler(WorkFitProjectDbContext context,
            IMediator mediator,
            ICurrentUserContext currentUser
        )
    {
        _context = context;
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(AssignTaskCommand command, CancellationToken ct)
    {
        var project = await _context.Projects.AsTracking()
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, ct)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, typeof(Domain.Entities.Project).Name, command.ProjectId);

        ProjectAccessGuard.EnsureAuthorized(project, _currentUser, ct);

        var existingTask = project.Tasks.FirstOrDefault(t => t.Id == command.TaskId)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);
        var proposedAllocation = command.AllocationPercentage ?? existingTask.AllocationPercentage;

        if (proposedAllocation == 0)
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "ASSIGNED_TASK_REQUIRES_ALLOCATION",
                "An assigned task must have a positive allocation percentage.",
                "Set a positive allocation percentage when assigning the task.");

        var isProjectMember = await _context.ProjectMembers.AsNoTracking().AnyAsync(
            member => member.ProjectId == command.ProjectId && member.EmployeeProfileId == command.AssigneeId,
            ct);
        if (!isProjectMember)
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "ASSIGNEE_NOT_PROJECT_MEMBER",
                "The selected employee is not a member of this project.",
                "Add the employee to this project before assigning the task.");

        await TaskAllocationCapacityValidator.ValidateAsync(
            _context,
            command.AssigneeId,
            proposedAllocation,
            existingTask.Id,
            ct);

        var task = project.AssignEmployeeForTask(command.TaskId, command.AssigneeId, command.AllocationPercentage);

        await _context.SaveChangesAsync(ct);
        await ProjectTaskStateEventPublisher.PublishAsync(_context, _mediator, task, "Assigned", ct);

       
        await _mediator.Publish(new TaskAssignedIntegrationEvent(
        task.Id, task.AssignedEmployeeId!.Value, task.AllocationPercentage), ct);


        return task.Id;
    }
}
