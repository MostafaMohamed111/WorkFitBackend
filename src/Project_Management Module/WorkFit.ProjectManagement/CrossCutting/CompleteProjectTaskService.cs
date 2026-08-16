using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Contracts.CompleteTaskService;
using WorkFit.ProjectManagement.Contracts.IntegrationEvents;
using WorkFit.ProjectManagement.CrossCutting;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.CrossCutting;

internal sealed class CompleteProjectTaskService : ICompleteProjectTaskService
{
    private readonly WorkFitProjectDbContext _context;
    private readonly IMediator _mediator;

    public CompleteProjectTaskService(WorkFitProjectDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<TaskCompletionContextDto> CompleteTaskAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
        if (task is null)
        {
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", taskId);
        }

        var project = await _context.Projects.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == task.ProjectId, cancellationToken);
        if (project is null)
        {
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Project", task.ProjectId);
        }

        if (!string.Equals(task.SourceSystem, SourceSystem.GitHub.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("This endpoint only supports GitHub-sourced tasks.");
        }

        var branchName = task.GitHubBranchName ?? task.SourceReferenceId;
        if (string.IsNullOrWhiteSpace(branchName))
        {
            throw new InvalidOperationException("The task branch reference is missing.");
        }

        if (!project.GitHubRepositoryId.HasValue || string.IsNullOrWhiteSpace(project.GitHubRepositoryName))
        {
            throw new InvalidOperationException("The project does not have a GitHub repository provisioned yet.");
        }

        task.Complete();

        var taskGitHub = await _context.TaskGitHubs.FirstOrDefaultAsync(x => x.TaskId == task.Id, cancellationToken);
        if (taskGitHub is not null)
        {
            taskGitHub.MarkCompleted(DateTimeOffset.UtcNow);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _mediator.Publish(
            new TaskCompletedIntegrationEvent(task.Id, task.AssignedEmployeeId, task.AllocationPercentage),
            cancellationToken);
        await ProjectTaskStateEventPublisher.PublishAsync(_context, _mediator, task, "Completed", cancellationToken);

        return new TaskCompletionContextDto(
            task.Id,
            task.ProjectId,
            project.OrganizationId,
            task.AssignedEmployeeId,
            project.TeamLeaderId,
            project.GitHubRepositoryName,
            branchName,
            task.AllocationPercentage);
    }
}