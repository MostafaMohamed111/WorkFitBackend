using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using TaskStatus = WorkFit.ProjectManagement.Domain.Enums.TaskStatus;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.DeleteTask;

public sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, DeleteTaskResponse>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public DeleteTaskCommandHandler(WorkFitProjectDbContext context, ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DeleteTaskResponse> Handle(DeleteTaskCommand command, CancellationToken ct)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == command.TaskId, ct);
        if (task is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);

        if (task.Status == TaskStatus.Done)
            throw new FeatureException(ModuleMarker.ModuleName, "TASK_ALREADY_COMPLETED",
                "Cannot delete a completed task.");

        var actorId = _currentUser.GetUserId(ct);
        _context.ProjectActivityLogs.Add(ProjectActivityLog.Create(
            task.ProjectId, actorId, ActivityActions.TaskDeleted, ActivityEntityType.Task,
            entityId: task.Id,
            beforeState: $"{{\"title\":\"{task.Title}\",\"status\":\"{task.Status}\"}}"));

        _context.ProjectTasks.Remove(task);
        await _context.SaveChangesAsync(ct);

        return new DeleteTaskResponse(true, task.Id);
    }
}