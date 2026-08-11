using WorkFit.SharedKernel.MediatorContract;
using WorkFit.CodeReview.Features.GitHubCodeReview;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.CompleteTask;

public sealed record TakeCompleteWithCodeReviewCommand(Guid TaskId) : IRequest<CodeReviewWorkflowExecutionResult>;
