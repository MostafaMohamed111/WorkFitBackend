using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.GetProjectAssignments;

public sealed record GetProjectAssignmentsQuery(
    Guid ProjectId,
    bool? IsActive
) : IRequest<List<ProjectAssignmentDto>>;
