using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Features.Project_Tasks.UpdateTask;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.GetTask;
public sealed class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDetailDto>
{
    private readonly WorkFitProjectDbContext _context;
    public GetTaskByIdQueryHandler(WorkFitProjectDbContext context) 
    {
        _context = context;
    }

    public async Task<TaskDetailDto> Handle(GetTaskByIdQuery query, CancellationToken ct)
    {
        var task = await _context.ProjectTasks.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == query.TaskId, ct);

        if (task is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", query.TaskId);

        return new TaskDetailDto(task.Id, task.ProjectId, task.Title, task.Description, task.TaskType,
            task.Status, task.Priority, task.AssigneeId, task.CreatedById, task.StoryPoints, task.DueDate,
            task.SourceSystem, task.SourceReferenceId, task.CompletedAt, task.CreatedAt);
    }
}