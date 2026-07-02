namespace WorkFit.ProjectManagement.Features.UpdateAssignment;

public sealed record UpdateAssignmentRequest(
    int? AllocationPercentage,
    string? RoleOnProject,
    DateOnly? EndDate
);
