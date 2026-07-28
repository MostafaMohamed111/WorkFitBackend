using WorkFit.CodeReview.Infrastructure.Services.Models;

namespace WorkFit.CodeReview.Infrastructure.Services;

public interface ICodeReviewAgentService
{
    Task<CodeReviewReviewerResult> ReviewAsync(
        CodeReviewReviewerConfig reviewer,
        string repository,
        string commitSha,
        string codeContext,
        CancellationToken ct);

    Task<CodeReviewSummaryResult> GenerateSummariesAsync(
        object aggregatedReviewData,
        CancellationToken ct);
}
