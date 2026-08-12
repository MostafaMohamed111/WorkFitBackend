namespace WorkFit.TalentManagement.Contracts.Indexing;

public sealed record EmployeeIndexingSnapshot(
    Guid EmployeeProfileId,
    Guid OrganizationId,
    string Name,
    string JobTitle,
    string? Bio,
    string Status,
    int CurrentAllocationPercentage,
    DateOnly? HireDate,
    IReadOnlyList<EmployeeSkillIndexingSnapshot> Skills,
    IReadOnlyList<EmployeeCertificationIndexingSnapshot> Certifications,
    EmployeeTaskPerformanceIndexingSnapshot? TaskPerformance,
    DateTimeOffset SnapshotAt);

public sealed record EmployeeSkillIndexingSnapshot(
    Guid SkillId,
    string Name,
    int ConfidenceScore,
    IReadOnlyList<EmployeeSkillEvidenceIndexingSnapshot> Evidence);

public sealed record EmployeeSkillEvidenceIndexingSnapshot(
    string Source,
    string Details,
    DateTime EvidenceDate);

public sealed record EmployeeCertificationIndexingSnapshot(
    string Name,
    string IssuingOrganization,
    DateOnly IssueDate,
    DateOnly? ExpiryDate,
    bool IsExpired);

public sealed record EmployeeTaskPerformanceIndexingSnapshot(
    int AssignedTaskCount,
    int CompletedTaskCount,
    int CompletedStoryPoints,
    DateTimeOffset? LastCompletedAt);
