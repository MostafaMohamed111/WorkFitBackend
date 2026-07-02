using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.CreateAssignment;

public sealed record CreateAssignmentCommand(
    Guid ProjectId,
    Guid EmployeeId,
    string? RoleOnProject,
    int AllocationPercentage,
    DateOnly StartDate,
    DateOnly? EndDate
) : IRequest<CreateAssignmentResponse>;
