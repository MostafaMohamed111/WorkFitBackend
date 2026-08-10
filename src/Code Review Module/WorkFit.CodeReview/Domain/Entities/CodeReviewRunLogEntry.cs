using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.CodeReview.Domain.Entities;

public sealed class CodeReviewRunLogEntry : BaseEntity
{
    public string ExecutionId { get; private set; } = string.Empty;
    public Guid? TaskId { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public string Organization { get; private set; } = string.Empty;
    public string Repository { get; private set; } = string.Empty;
    public string Branch { get; private set; } = string.Empty;
    public string CommitSha { get; private set; } = string.Empty;
    public string PullRequestNumber { get; private set; } = string.Empty;
    public int OverallScore { get; private set; }
    public string Risk { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string ErrorMessage { get; private set; } = string.Empty;
    public DateTime LoggedAt { get; private set; }

    private CodeReviewRunLogEntry() : base()
    {
    }

    public static CodeReviewRunLogEntry CreateSuccess(
        string executionId,
        string organization,
        string repository,
        string branch,
        string commitSha,
        string pullRequestNumber,
        Guid? taskId,
        Guid? employeeId,
        int overallScore,
        string risk,
        string summary,
        DateTime loggedAt)
    {
        return new CodeReviewRunLogEntry
        {
            ExecutionId = executionId,
            TaskId = taskId,
            EmployeeId = employeeId,
            Organization = organization,
            Repository = repository,
            Branch = branch,
            CommitSha = commitSha,
            PullRequestNumber = pullRequestNumber,
            OverallScore = overallScore,
            Risk = risk,
            Status = "success",
            Summary = summary,
            ErrorMessage = string.Empty,
            LoggedAt = loggedAt
        };
    }

    public static CodeReviewRunLogEntry CreateFailure(
        string executionId,
        string workflowName,
        string stageName,
        string errorMessage,
        Guid? taskId,
        Guid? employeeId,
        DateTime loggedAt)
    {
        return new CodeReviewRunLogEntry
        {
            ExecutionId = executionId,
            TaskId = taskId,
            EmployeeId = employeeId,
            Organization = string.Empty,
            Repository = string.Empty,
            Branch = string.Empty,
            CommitSha = string.Empty,
            PullRequestNumber = string.Empty,
            OverallScore = 0,
            Risk = string.Empty,
            Status = "error",
            Summary = $"Workflow failed: {workflowName}",
            ErrorMessage = string.IsNullOrWhiteSpace(stageName)
                ? errorMessage
                : $"[{stageName}] {errorMessage}",
            LoggedAt = loggedAt
        };
    }
}
