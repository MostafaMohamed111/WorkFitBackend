using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.UpdateAssignment;

public sealed record UpdateAssignmentCommand(
    Guid ProjectId,
    Guid AssignmentId,
    int? AllocationPercentage,
    string? RoleOnProject,
    DateOnly? EndDate
) : IRequest;
