using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkFit.CodeReview.Features.GitHubCodeReview;
using WorkFit.Organizations.Features.OrganizationsMe;
using WorkFit.ProjectManagement.Contracts.IntegrationEvents;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.CompleteTask;

public sealed class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, Guid>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<CompleteTaskCommandHandler> _logger;

    public CompleteTaskCommandHandler(
        WorkFitProjectDbContext context,
        IMediator mediator,
        ILogger<CompleteTaskCommandHandler> logger)
    {
        _context = context;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Guid> Handle(CompleteTaskCommand command, CancellationToken ct)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == command.TaskId, ct);
        if (task is null)
        {
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);
        }

        task.Complete();

        var taskGitHub = await _context.TaskGitHubs.FirstOrDefaultAsync(x => x.TaskId == task.Id, ct);
        if (taskGitHub is not null)
        {
            taskGitHub.MarkCompleted(DateTimeOffset.UtcNow);
        }

        await _context.SaveChangesAsync(ct);

        await _mediator.Publish(
            new TaskCompletedIntegrationEvent(task.Id, task.AssignedEmployeeId!.Value, task.AllocationPercentage),
            ct);

        if (taskGitHub is not null &&
            !string.IsNullOrWhiteSpace(taskGitHub.GitHubRepositoryName) &&
            (taskGitHub.GitHubPullRequestNumber.HasValue || !string.IsNullOrWhiteSpace(taskGitHub.GitHubBranchName)))
        {
            var project = await _context.Projects.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == task.ProjectId, ct);

            if (project is null)
            {
                throw new EntityNotFoundException(ModuleMarker.ModuleName, "Project", task.ProjectId);
            }

            var organizationGitHub = await _mediator.Send(new GetOrganizationGitHubInfoQuery(project.OrganizationId), ct);
            if (!string.IsNullOrWhiteSpace(organizationGitHub.GitHubOrganizationLogin))
            {
                try
                {
                    await _mediator.Send(
                        new ReviewTaskGitHubChangesCommand(
                            task.Id,
                            task.AssignedEmployeeId,
                            organizationGitHub.GitHubOrganizationLogin,
                            taskGitHub.GitHubRepositoryName,
                            taskGitHub.GitHubBranchName,
                            taskGitHub.GitHubPullRequestNumber,
                            null),
                        ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Task-scoped code review failed for task {TaskId}.", task.Id);
                }
            }
            else
            {
                _logger.LogInformation(
                    "Skipping code review for task {TaskId} because the organization is not connected to GitHub.",
                    task.Id);
            }
        }

        return task.Id;
    }
}
