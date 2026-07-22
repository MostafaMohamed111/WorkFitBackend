using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.GetProjectTasks;

public sealed class GetProjectTasksQueryHandler : IRequestHandler<GetProjectTasksQuery, List<TaskListItemDto>>
{
    private readonly WorkFitProjectDbContext _context;

    public GetProjectTasksQueryHandler(WorkFitProjectDbContext context)
    {
       _context = context;
    }

    public async Task<List<TaskListItemDto>> Handle(GetProjectTasksQuery query, CancellationToken ct)
    {
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == query.ProjectId, ct);
        if (!projectExists)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Project", query.ProjectId);

        var tasksQuery = _context.ProjectTasks.Where(t => t.ProjectId == query.ProjectId);

        if (query.Status.HasValue) tasksQuery = tasksQuery.Where(t => t.Status == query.Status.Value);
        if (query.AssigneeId.HasValue) tasksQuery = tasksQuery.Where(t => t.AssignedEmployeeId == query.AssigneeId.Value);
        if (query.TaskType.HasValue) tasksQuery = tasksQuery.Where(t => t.TaskType == query.TaskType.Value);
        if (query.Priority.HasValue) tasksQuery = tasksQuery.Where(t => t.Priority == query.Priority.Value);

        return await tasksQuery
            .Select(t => new TaskListItemDto(t.Id, t.Title, t.TaskType, t.Status, t.Priority,
                t.AssignedEmployeeId, t.StoryPoints, t.DueDate, t.CompletedAt))
            .ToListAsync(ct);
    }
}