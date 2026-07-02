using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.RemoveAssignment;

public sealed record RemoveAssignmentCommand(
    Guid ProjectId,
    Guid AssignmentId
) : IRequest;
