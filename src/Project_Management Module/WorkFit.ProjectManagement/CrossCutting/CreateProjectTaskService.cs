using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Contracts.CreateProjectService;
using WorkFit.ProjectManagement.Contracts.CreateProjectTaskService;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Infrastructure;

namespace WorkFit.ProjectManagement.CrossCutting;

internal sealed class CreateProjectTaskService : ICreateProjectTaskService
{
    private readonly WorkFitProjectDbContext _dbContext;

    public CreateProjectTaskService(WorkFitProjectDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> UpsertExternalTaskAsync(UpsertExternalTaskDto dto, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<SourceSystem>(dto.SourceSystem, true, out var sourceSystem))
            sourceSystem = SourceSystem.Internal;

        if (!Enum.TryParse<Domain.Enums.TaskType>(dto.TaskType, true, out var taskType))
            taskType = Domain.Enums.TaskType.Task;

        if (!Enum.TryParse<Domain.Enums.TaskPriority>(dto.Priority, true, out var priority))
            priority = Domain.Enums.TaskPriority.Medium;

        var task = await _dbContext.ProjectTasks
            .FirstOrDefaultAsync(t => t.SourceSystem == sourceSystem.ToString() && t.SourceReferenceId == dto.SourceReferenceId, cancellationToken);

        if (task is null)
        {
            task = ProjectTask.Create(
                projectId: dto.ProjectId,
                title: dto.Title ?? "(no title)",
                description: dto.Description,
                taskType: taskType,
                priority: priority,
                createdById: Guid.Empty, // external tasks might not have a creator initially
                assigneeId: dto.AssigneeId,
                storyPoints: dto.StoryPoints,
                dueDate: null
            );

            task.SetSource(sourceSystem.ToString(), dto.SourceReferenceId);

            ApplyTaskStatus(task, dto.Status);
            SetHistoricalTimestamps(task, dto.CompletedAt); // CompletedAt or UpdatedAt

            _dbContext.ProjectTasks.Add(task);
        }
        else
        {
            // Update logic can be added if needed, right now we just skip or update basic info
            // For simplicity, we assume UPSERT means create if not exists
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return task.Id;
    }

    private static void ApplyTaskStatus(ProjectTask task, string statusName)
    {
        if (!Enum.TryParse<Domain.Enums.TaskStatus>(statusName, true, out var status))
            return;

        if (status == Domain.Enums.TaskStatus.Done)
        {
            try { task.Complete(); } catch { }
        }
        else if (status != Domain.Enums.TaskStatus.ToDo)
        {
            try { task.ChangeStatus(status); } catch { }
        }
    }

    private static void SetHistoricalTimestamps(ProjectTask task, DateTimeOffset? resolvedAt)
    {
        var type = typeof(ProjectTask);
        if (resolvedAt.HasValue)
        {
            var completedProp = type.GetProperty("CompletedAt");
            if (completedProp != null && completedProp.CanWrite)
            {
                 completedProp.SetValue(task, resolvedAt.Value);
            }
        }
    }
}
