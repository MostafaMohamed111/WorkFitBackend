using WorkFit.CodeReview.Contracts.GitHubCodeReview;
using WorkFit.CodeReview.Infrastructure.Services.Models;

namespace WorkFit.CodeReview.Features.GitHubCodeReview;

public sealed record CodeReviewWorkflowExecutionResult(
    string ExecutionId,
    CodeReviewResultDto Response,
    string ExecutiveSummary,
    string DeveloperSummary,
    bool HasReviewableFiles,
    bool Truncated);
