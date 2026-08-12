namespace WorkFit.Rag.Contracts.Indexing;

public sealed record EmployeeProfileIndexDocument(
    Guid EmployeeProfileId,
    Guid OrganizationId,
    string EmployeeName,
    string Status,
    double AvailableAllocation,
    double PerformanceScore,
    string ProfileSummary,
    IReadOnlyList<EmployeeSkillIndexDocument> Skills);

public sealed record EmployeeSkillIndexDocument(Guid? SkillId, string Name, double Level);

public sealed record ProjectTaskIndexDocument(
    Guid TaskId,
    Guid ProjectId,
    Guid OrganizationId,
    string TaskTitle,
    string? TaskDescription,
    string TaskType,
    string Status,
    string Priority,
    int? StoryPoints,
    DateOnly? DueDate,
    string? ProjectName,
    string? ProjectDescription,
    string ProjectStatus,
    IReadOnlyList<string> RequiredSkills,
    IReadOnlyList<EmployeeTaskOutcomeIndexDocument> EmployeeOutcomes,
    bool IsActive,
    int Revision,
    DateTimeOffset OccurredAt);

public sealed record EmployeeTaskOutcomeIndexDocument(Guid EmployeeProfileId, double PerformanceScore);
