namespace WorkFit.CodeReview.Contracts.GitHubCodeReview;

public interface IReviewTaskGitHub
{
    Task<CodeReviewWorkflowExecutionResult> ReviewTaskAsync(
        Guid taskId,
        Guid? employeeId,
        string organization,
        string repository,
        string? branch,
        int? pullRequestNumber,
        string? accessToken,
        CancellationToken cancellationToken = default);
}