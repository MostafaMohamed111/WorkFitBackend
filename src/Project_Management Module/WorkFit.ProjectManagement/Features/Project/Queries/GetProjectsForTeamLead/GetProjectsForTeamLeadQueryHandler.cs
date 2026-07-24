using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Features.Project.Queries.Dtos;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.Queries.GetProjectsForTeamLead;

internal sealed class GetProjectsForTeamLeadQueryHandler : IRequestHandler<GetProjectsForTeamLeadQuery, IReadOnlyList<ProjectListItemDto>>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly ICurrentUserContext _currentUserContext;

    public GetProjectsForTeamLeadQueryHandler(WorkFitProjectDbContext context, ICurrentUserContext currentUserContext)
    {
        _context = context;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyList<ProjectListItemDto>> Handle(GetProjectsForTeamLeadQuery querry, CancellationToken cancellationToken)
    {
        var teamLeadId = _currentUserContext.GetUserId();
        var projects = await _context.Projects.AsNoTracking()
            .Include(p => p.Tasks)
            .Where(p => p.TeamLeaderId == teamLeadId)
            .Select(p => new ProjectListItemDto
            (
                p.Id,
                p.Name,
                p.OrganizationId,
                p.Status,
                p.StartDate,
                p.EndDate,
                p.AssignedEmployees.Count(),
                p.Tasks.Count()
            ))
            .ToListAsync(cancellationToken);

        return projects.AsReadOnly();
    }
}