using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.AssignTask;
public sealed class AssignTaskCommandHandler : IRequestHandler<AssignTaskCommand, AssignTaskResponse>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public AssignTaskCommandHandler(WorkFitProjectDbContext context, ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AssignTaskResponse> Handle(AssignTaskCommand command, CancellationToken ct)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == command.TaskId, ct);
        if (task is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);

        var isMemberOfProject = await _context.ProjectAssignments.AnyAsync(
            a => a.ProjectId == task.ProjectId &&
                 a.EmployeeId == command.AssigneeId &&
                 a.IsActive, ct);

        if (!isMemberOfProject)
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "EMPLOYEE_NOT_ASSIGNED_TO_PROJECT",
                $"Employee '{command.AssigneeId}' is not an active member of project '{task.ProjectId}'.",
                "You can only assign a task to an employee who is a member of this project.");

        var oldAssigneeId = task.AssigneeId;
        task.Assign(command.AssigneeId);

        var actorId = _currentUser.GetUserId(ct);
        _context.ProjectActivityLogs.Add(ProjectActivityLog.Create(
            task.ProjectId, actorId, ActivityActions.TaskAssigned, ActivityEntityType.Task,
            entityId: task.Id,
            beforeState: $"{{\"assigneeId\":\"{oldAssigneeId}\"}}",
            afterState: $"{{\"assigneeId\":\"{command.AssigneeId}\"}}"));

        await _context.SaveChangesAsync(ct);

        return new AssignTaskResponse(task.Id, task.AssigneeId!.Value);
    }
}