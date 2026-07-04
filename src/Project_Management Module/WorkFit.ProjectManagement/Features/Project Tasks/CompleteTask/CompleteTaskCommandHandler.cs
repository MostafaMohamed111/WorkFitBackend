using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.CompleteTask;
public sealed class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, CompleteTaskResponse>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public CompleteTaskCommandHandler(WorkFitProjectDbContext context, ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CompleteTaskResponse> Handle(CompleteTaskCommand command, CancellationToken ct)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == command.TaskId, ct);
        if (task is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);

        task.Complete();

        var actorId = _currentUser.GetUserId(ct);
        _context.ProjectActivityLogs.Add(ProjectActivityLog.Create(
            task.ProjectId, actorId, ActivityActions.TaskCompleted, ActivityEntityType.Task,
            entityId: task.Id,
            afterState: $"{{\"completedAt\":\"{task.CompletedAt:o}\"}}"));

        await _context.SaveChangesAsync(ct);

        return new CompleteTaskResponse(task.Id, task.Status.ToString(), task.CompletedAt!.Value);
    }
}