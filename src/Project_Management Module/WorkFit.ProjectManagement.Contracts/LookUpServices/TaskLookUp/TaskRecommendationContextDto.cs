namespace WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;

public sealed record TaskRecommendationContextDto(
    Guid Id,
    string Title,
    string? Description,
    string TaskType,
    string Status,
    string Priority,
    int? StoryPoints,
    DateOnly? DueDate,
    int AllocationPercentage,
    Guid? AssignedEmployeeId,
    Guid CreatedById,
    Guid ProjectId,
    string ProjectName,
    string? ProjectDescription,
    string ProjectStatus,
    DateOnly? ProjectStartDate,
    DateOnly? ProjectEndDate,
    Guid OrganizationId,
    Guid? TeamLeaderId,
    IReadOnlyList<ProjectRequiredSkillContextDto> ProjectRequiredSkills,
    string? SourceSystem,
    string? SourceReferenceId,
    string? GitHubBranchName,
    string? GitHubBranchNodeId,
    int Revision,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? DeletedAt,
    bool IsDeleted,
    bool IsActive
);

public sealed record ProjectRequiredSkillContextDto(
    Guid SkillId,
    string Level,
    int Priority);
