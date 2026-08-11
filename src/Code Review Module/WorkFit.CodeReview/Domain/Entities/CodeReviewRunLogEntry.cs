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
            ExecutionId = Truncate(executionId, 100),
            TaskId = taskId,
            EmployeeId = employeeId,
            Organization = Truncate(organization, 200),
            Repository = Truncate(repository, 200),
            Branch = Truncate(branch, 200),
            CommitSha = Truncate(commitSha, 100),
            PullRequestNumber = Truncate(pullRequestNumber, 50),
            OverallScore = overallScore,
            Risk = Truncate(risk, 50),
            Status = "success",
            Summary = Truncate(summary, 4000),
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
            ExecutionId = Truncate(executionId, 100),
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
            Summary = Truncate($"Workflow failed: {workflowName}", 4000),
            ErrorMessage = Truncate(string.IsNullOrWhiteSpace(stageName)
                ? errorMessage
                : $"[{stageName}] {errorMessage}", 4000),
            LoggedAt = loggedAt
        };
    }

    private static string Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
