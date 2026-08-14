
using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.ProjectManagement.CrossCutting;

internal sealed class TaskLookUpService : ITaskLookUpService
{
    private readonly WorkFitProjectDbContext _dbContext;

    public TaskLookUpService(WorkFitProjectDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskRecommendationContextDto> GetRecommendationContextAsync(
        Guid taskId,
        CancellationToken cancellation)
    {
        var context = await (
            from projectTask in _dbContext.ProjectTasks.IgnoreQueryFilters().AsNoTracking()
            join parentProject in _dbContext.Projects.AsNoTracking() on projectTask.ProjectId equals parentProject.Id
            where projectTask.Id == taskId
            select new { Task = projectTask, Project = parentProject })
            .FirstOrDefaultAsync(cancellation);

        if (context is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", taskId);

        var requiredSkills = await _dbContext.ProjectRequiredSkills
            .AsNoTracking()
            .Where(skill => skill.ProjectId == context.Project.Id)
            .OrderBy(skill => skill.Priority)
            .Select(skill => new ProjectRequiredSkillContextDto(
                skill.SkillId,
                skill.Level.ToString(),
                skill.Priority))
            .ToListAsync(cancellation);

        var task = context.Task;
        var project = context.Project;

        return new TaskRecommendationContextDto(
            task.Id,
            task.Title,
            task.Description,
            task.TaskType.ToString(),
            task.Status.ToString(),
            task.Priority.ToString(),
            task.StoryPoints,
            task.DueDate,
            task.AllocationPercentage,
            task.AssignedEmployeeId,
            task.CreatedById,
            project.Id,
            project.Name,
            project.Description,
            project.Status.ToString(),
            project.StartDate,
            project.EndDate,
            project.OrganizationId,
            project.TeamLeaderId,
            requiredSkills,
            task.SourceSystem,
            task.SourceReferenceId,
            task.GitHubBranchName,
            task.GitHubBranchNodeId,
            task.Revision,
            task.CreatedAt,
            task.UpdatedAt,
            task.CompletedAt,
            task.DeletedAt,
            task.IsDeleted,
            task.IsActive);
    }
}
