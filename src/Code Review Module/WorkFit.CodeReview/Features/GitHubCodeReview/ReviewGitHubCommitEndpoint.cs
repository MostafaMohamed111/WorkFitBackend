using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.CodeReview.Contracts.GitHubCodeReview;
using WorkFit.CodeReview.Infrastructure.Services;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.CodeReview.Features.GitHubCodeReview;

public sealed class ReviewGitHubCommitEndpoint : Endpoint<ReviewGitHubCommitRequest, CodeReviewResultDto>
{
    private readonly IMediator _mediator;

    public ReviewGitHubCommitEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/code-review/github-code-review");
        AllowAnonymous();
        Options(x => x.WithTags("Code Review"));
        Description(static b => b
            .Produces<CodeReviewResultDto>(200)
            .Produces<ReviewGitHubCommitBadRequestResponse>(400));
    }

    public override async Task HandleAsync(ReviewGitHubCommitRequest req, CancellationToken ct)
    {
        var missingFields = GetMissingFields(req);
        if (missingFields.Count > 0)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await HttpContext.Response.WriteAsJsonAsync(
                new ReviewGitHubCommitBadRequestResponse("Missing required fields", missingFields),
                cancellationToken: ct);
            return;
        }

        var command = new ReviewGitHubCommitCommand(
            req.organization.Trim(),
            req.repository.Trim(),
            req.branch.Trim(),
            req.commitSha.Trim(),
            req.pullRequestNumber,
            req.accessToken?.Trim());

        var result = await _mediator.Send(command, ct);

        if (!result.HasReviewableFiles)
        {
            await Send.OkAsync(result.Response, cancellation: ct);
            return;
        }

        await Send.OkAsync(result.Response, cancellation: ct);
    }

    private static List<string> GetMissingFields(ReviewGitHubCommitRequest req)
    {
        var missingFields = new List<string>();
        if (string.IsNullOrWhiteSpace(req.organization)) missingFields.Add(nameof(req.organization));
        if (string.IsNullOrWhiteSpace(req.repository)) missingFields.Add(nameof(req.repository));
        if (string.IsNullOrWhiteSpace(req.branch)) missingFields.Add(nameof(req.branch));
        if (string.IsNullOrWhiteSpace(req.commitSha)) missingFields.Add(nameof(req.commitSha));
        if (string.IsNullOrWhiteSpace(req.accessToken)) missingFields.Add(nameof(req.accessToken));
        return missingFields;
    }
}
