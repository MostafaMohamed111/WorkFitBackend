namespace WorkFit.ProjectManagement.Features.GetProjectAssignments;

public sealed record ProjectAssignmentDto(
    Guid Id,
    Guid EmployeeId,
    string? RoleOnProject,
    int AllocationPercentage,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    DateTimeOffset JoinedAt
);
