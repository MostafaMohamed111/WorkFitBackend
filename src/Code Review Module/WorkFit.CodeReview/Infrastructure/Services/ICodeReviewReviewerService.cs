using WorkFit.CodeReview.Infrastructure.Services.Models;

namespace WorkFit.CodeReview.Infrastructure.Services;

public interface ICodeReviewReviewerService
{
    Task<IReadOnlyList<CodeReviewReviewerResult>> RunReviewersAsync(
        IReadOnlyList<CodeReviewReviewerConfig> reviewers,
        string repository,
        string commitSha,
        string codeContext,
        CancellationToken ct);
}
