using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.ProjectManagement.Domain.Entities;

public sealed class TaskGitHub : BaseEntity
{
    public Guid TaskId { get; private set; }
    public long? GitHubRepositoryId { get; private set; }
    public string? GitHubRepositoryName { get; private set; }
    public string? GitHubBranchName { get; private set; }
    public int? GitHubPullRequestNumber { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public ProjectTask Task { get; private set; } = default!;

    private TaskGitHub() : base()
    {
    }

    public static TaskGitHub Create(
        Guid taskId,
        long? githubRepositoryId,
        string? githubRepositoryName,
        string? githubBranchName,
        int? githubPullRequestNumber)
    {
        if (taskId == Guid.Empty)
        {
            throw new ArgumentException("Task id is required.", nameof(taskId));
        }

        return new TaskGitHub
        {
            TaskId = taskId,
            GitHubRepositoryId = githubRepositoryId,
            GitHubRepositoryName = githubRepositoryName?.Trim(),
            GitHubBranchName = githubBranchName?.Trim(),
            GitHubPullRequestNumber = githubPullRequestNumber
        };
    }

    public void Update(
        long? githubRepositoryId,
        string? githubRepositoryName,
        string? githubBranchName,
        int? githubPullRequestNumber)
    {
        GitHubRepositoryId = githubRepositoryId;
        GitHubRepositoryName = githubRepositoryName?.Trim();
        GitHubBranchName = githubBranchName?.Trim();
        GitHubPullRequestNumber = githubPullRequestNumber;
        MarkUpdated();
    }

    public void MarkCompleted(DateTimeOffset completedAt)
    {
        CompletedAt = completedAt;
        MarkUpdated();
    }
}
