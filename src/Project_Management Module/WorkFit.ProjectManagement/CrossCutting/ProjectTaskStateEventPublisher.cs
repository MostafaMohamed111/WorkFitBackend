using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Contracts.IntegrationEvents;
using WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.CrossCutting;

internal static class ProjectTaskStateEventPublisher
{
    public static async Task PublishAsync(
        WorkFitProjectDbContext dbContext,
        IMediator mediator,
        ProjectTask task,
        string changeType,
        CancellationToken cancellationToken)
    {
        var project = await dbContext.Projects
            .AsNoTracking()
            .Include(x => x.RequiredSkills)
            .FirstOrDefaultAsync(x => x.Id == task.ProjectId, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, "Project", task.ProjectId);

        await mediator.Publish(new ProjectTaskStateChangedIntegrationEvent(
            task.Id,
            task.ProjectId,
            project.OrganizationId,
            project.TeamLeaderId,
            project.Name,
            project.Description,
            project.Status.ToString(),
            project.StartDate,
            project.EndDate,
            project.SourceSystem?.ToString(),
            project.SourceReferenceId,
            project.GitHubRepositoryId,
            project.GitHubRepositoryName,
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
            project.RequiredSkills
                .OrderBy(x => x.Priority)
                .Select(x => new ProjectRequiredSkillContextDto(x.SkillId, x.Level.ToString(), x.Priority))
                .ToArray(),
            task.SourceSystem,
            task.SourceReferenceId,
            task.GitHubBranchName,
            task.GitHubBranchNodeId,
            task.CreatedAt,
            task.UpdatedAt,
            task.CompletedAt,
            task.DeletedAt,
            task.IsDeleted,
            task.IsActive,
            task.Revision,
            changeType,
            DateTimeOffset.UtcNow), cancellationToken);
    }
}
