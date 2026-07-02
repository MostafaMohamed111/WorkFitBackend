namespace WorkFit.ProjectManagement.Features.CreateAssignment;

public sealed record CreateAssignmentRequest(
    Guid EmployeeId,
    string? RoleOnProject,
    int AllocationPercentage,
    DateOnly StartDate,
    DateOnly? EndDate
);
