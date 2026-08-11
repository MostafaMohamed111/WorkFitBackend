using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkFit.CodeReview.Features.GitHubCodeReview;
using WorkFit.Organizations.Features.OrganizationsMe;
using WorkFit.ProjectManagement.Contracts.IntegrationEvents;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.CompleteTask;

public sealed class TakeCompleteWithCodeReviewCommandHandler : IRequestHandler<TakeCompleteWithCodeReviewCommand, CodeReviewWorkflowExecutionResult>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<TakeCompleteWithCodeReviewCommandHandler> _logger;

    public TakeCompleteWithCodeReviewCommandHandler(
        WorkFitProjectDbContext context,
        IMediator mediator,
        ILogger<TakeCompleteWithCodeReviewCommandHandler> logger)
    {
        _context = context;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<CodeReviewWorkflowExecutionResult> Handle(TakeCompleteWithCodeReviewCommand command, CancellationToken ct)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == command.TaskId, ct);
        if (task is null)
        {
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);
        }

        var project = await _context.Projects.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == task.ProjectId, ct);

        if (project is null)
        {
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Project", task.ProjectId);
        }

        task.Complete();
        await _context.SaveChangesAsync(ct);

        await _mediator.Publish(
            new TaskCompletedIntegrationEvent(task.Id, task.AssignedEmployeeId!.Value, task.AllocationPercentage),
            ct);

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

        var organizationGitHub = await _mediator.Send(new GetOrganizationGitHubInfoQuery(project.OrganizationId), ct);
        if (string.IsNullOrWhiteSpace(organizationGitHub.GitHubOrganizationLogin))
        {
            throw new InvalidOperationException("The organization is not connected to GitHub.");
        }

        var reviewResult = await _mediator.Send(
            new ReviewTaskGitHubChangesCommand(
                task.Id,
                task.AssignedEmployeeId,
                organizationGitHub.GitHubOrganizationLogin,
                project.GitHubRepositoryName,
                branchName,
                null,
                null),
            ct);

        _logger.LogInformation(
            "Completed take-complete-with-code-review for task {TaskId} in repository {RepositoryId}.",
            task.Id,
            project.GitHubRepositoryId);

        return reviewResult;
    }
}
