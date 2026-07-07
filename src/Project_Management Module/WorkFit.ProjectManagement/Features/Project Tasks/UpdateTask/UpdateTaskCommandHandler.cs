using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.UpdateTask;
public sealed class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDetailDto>
{
    private readonly WorkFitProjectDbContext _context;

    public UpdateTaskCommandHandler(WorkFitProjectDbContext context)
    {
        _context = context;
    }

    public async Task<TaskDetailDto> Handle(UpdateTaskCommand command, CancellationToken ct)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == command.TaskId, ct);
        if (task is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);

        task.UpdateDetails(command.Title, command.Description, command.Priority, command.StoryPoints, command.DueDate);

        if (command.Status.HasValue)
            task.ChangeStatus(command.Status.Value);

        await _context.SaveChangesAsync(ct);

        return new TaskDetailDto(task.Id, task.ProjectId, task.Title, task.Description, task.TaskType,
            task.Status, task.Priority, task.AssigneeId, task.CreatedById, task.StoryPoints, task.DueDate,
            task.SourceSystem, task.SourceReferenceId, task.CompletedAt, task.CreatedAt);
    }
}