using WorkFit.Rag.Contracts.Recommendations;

namespace WorkFit.Rag.Contracts.SkillGainAnalysis;

public interface ISkillGainAnalysisService
{
    Task<SkillGainAnalysisResponse> AnalyzeAsync(
        SkillGainAnalysisContext context,
        CancellationToken cancellationToken = default);
}

public sealed record SkillGainAnalysisContext(
    Guid TaskId,
    Guid ProjectId,
    Guid OrganizationId,
    string TaskTitle,
    string? TaskDescription,
    string ProjectName,
    string? ProjectDescription,
    IReadOnlyList<RequiredSkill> RequiredSkills,
    Guid EmployeeProfileId,
    string EmployeeName,
    string? EmployeeJobTitle,
    IReadOnlyList<EmployeeSkillGainInput> EmployeeSkills,
    CodeReviewGainInput? CodeReview);

public sealed record EmployeeSkillGainInput(
    Guid SkillId,
    string Name,
    int ConfidenceScore);

public sealed record CodeReviewGainInput(
    int? OverallScore,
    string Risk,
    string TechnicalDebt,
    string ExecutiveSummary,
    string DeveloperSummary,
    IReadOnlyList<string> PositiveFindings,
    IReadOnlyList<CodeReviewIssueGainInput> Issues,
    IReadOnlyList<string> Recommendations);

public sealed record CodeReviewIssueGainInput(
    string Title,
    string Severity,
    string Detail,
    string File);

public sealed record SkillGainAnalysisResponse(
    IReadOnlyList<SkillGainChange> SkillChanges,
    IReadOnlyList<NewSkillGain> NewSkills);

public sealed record SkillGainChange(
    Guid SkillId,
    string SkillName,
    int OldScore,
    int NewScore,
    string Reasoning);

public sealed record NewSkillGain(
    string SkillName,
    int NewScore,
    string Reasoning);