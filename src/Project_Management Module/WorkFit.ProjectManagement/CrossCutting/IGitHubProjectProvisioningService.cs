using WorkFit.CodeReview.Infrastructure.Services.Models;

namespace WorkFit.ProjectManagement.CrossCutting;

public interface IGitHubProjectProvisioningService
{
    Task<GitHubRepositoryCreationResult> CreateProjectRepositoryAsync(
        Guid organizationId,
        Guid projectId,
        string projectName,
        CancellationToken cancellationToken = default);

    Task<GitHubBranchCreationResult> CreateTaskBranchAsync(
        Guid organizationId,
        Guid projectId,
        string? repositoryName,
        string projectName,
        string taskName,
        Guid taskId,
        CancellationToken cancellationToken = default);
}
