namespace WorkFit.CodeReview.Contracts.GitHubCodeReview;

public sealed record CodeReviewWorkflowExecutionResult(
    string ExecutionId,
    CodeReviewResultDto Response,
    string ExecutiveSummary,
    string DeveloperSummary,
    bool HasReviewableFiles,
    bool Truncated);