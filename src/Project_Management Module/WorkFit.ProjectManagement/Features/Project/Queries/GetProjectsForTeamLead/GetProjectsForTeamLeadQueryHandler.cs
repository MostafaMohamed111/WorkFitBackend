using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.ProjectManagement.Features.Project.Queries.Dtos;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.Queries.GetProjectsForTeamLead;

internal sealed class GetProjectsForTeamLeadQueryHandler : IRequestHandler<GetProjectsForTeamLeadQuery, IReadOnlyList<ProjectListItemDto>>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGetOrganizationIdService _organizations;

    public GetProjectsForTeamLeadQueryHandler(
        WorkFitProjectDbContext context,
        ICurrentUserContext currentUserContext,
        IGetOrganizationIdService organizations)
    {
        _context = context;
        _currentUserContext = currentUserContext;
        _organizations = organizations;
    }

    public async Task<IReadOnlyList<ProjectListItemDto>> Handle(GetProjectsForTeamLeadQuery querry, CancellationToken cancellationToken)
    {
        var teamLeadId = _currentUserContext.GetUserId();

        Guid? userOrgId = null;
        try
        {
            userOrgId = await _organizations.GetOrganizationIdAsync(teamLeadId, cancellationToken);
        }
        catch
        {
            // Fallback if organization lookup returns not found for user ID
        }

        var query = _context.Projects.AsNoTracking()
            .Include(p => p.Tasks)
            .Include(p => p.Members)
            .Where(p => p.TeamLeaderId == teamLeadId ||
                        (userOrgId.HasValue && p.OrganizationId == userOrgId.Value) ||
                        p.TeamLeaderId == null ||
                        p.TeamLeaderId == Guid.Empty);

        if (Enum.TryParse<ProjectStatus>(querry.Status, true, out var status))
        {
            query = query.Where(p => p.Status == status);
        }
        else
        {
            query = query.Where(p => p.Status != ProjectStatus.Cancelled);
        }

        var projects = await query
            .Select(p => new ProjectListItemDto
            (
                p.Id,
                p.Name,
                p.OrganizationId,
                p.Status,
                p.StartDate,
                p.EndDate,
                p.Members.Count,
                p.Tasks.Count()
            ))
            .ToListAsync(cancellationToken);

        return projects.AsReadOnly();
    }
}
