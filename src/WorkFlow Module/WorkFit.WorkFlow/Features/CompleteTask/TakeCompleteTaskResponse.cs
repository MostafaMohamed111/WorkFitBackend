using WorkFit.CodeReview.Contracts.GitHubCodeReview;
using WorkFit.Rag.Contracts.SkillGainAnalysis;

namespace WorkFit.WorkFlow.Features.CompleteTask;

public sealed record TakeCompleteTaskResponse(
    Guid TaskId,
    CodeReviewWorkflowExecutionResult CodeReview,
    SkillGainAnalysisResponse SkillGainAnalysis,
    Guid? AssessmentId);