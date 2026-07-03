using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.GetProjectAssignments;

public sealed class GetProjectAssignmentsQueryHandler : IRequestHandler<GetProjectAssignmentsQuery, List<ProjectAssignmentDto>>
{
    private readonly WorkFitProjectDbContext _context;

    public GetProjectAssignmentsQueryHandler(WorkFitProjectDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectAssignmentDto>> Handle(GetProjectAssignmentsQuery query, CancellationToken ct)
    {
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == query.ProjectId, ct);
        if (!projectExists)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Project", query.ProjectId);

        var assignmentsQuery = _context.ProjectAssignments
            .Where(a => a.ProjectId == query.ProjectId);

        if (query.IsActive.HasValue)
            assignmentsQuery = assignmentsQuery.Where(a => a.IsActive == query.IsActive.Value);

        var assignments = await assignmentsQuery
            .Select(a => new ProjectAssignmentDto(
                a.Id,
                a.EmployeeId,
                a.RoleOnProject,
                a.AllocationPercentage,
                a.StartDate,
                a.EndDate,
                a.IsActive,
                a.JoinedAt))
            .ToListAsync(ct);

        return assignments;
    }
}
