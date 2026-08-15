using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployees;

public sealed class GetEmployeesEndpoint
    : EndpointWithoutRequest<List<EmployeeListItemDto>>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly TalentDbContext _context;

    public GetEmployeesEndpoint(
        IMediator mediator,
        ICurrentUserContext currentUserContext,
        TalentDbContext context)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
        _context = context;
    }

    public override void Configure()
    {
        Get("/api/employees");
        Roles("Admin", "HR", "OrganizationOwner", "TeamLeader", "Employee");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orgId = Guid.Empty;

        // 1. Check Query parameter or Claims
        var queryOrgId = Query<string>("orgId", isRequired: false);
        if (!string.IsNullOrEmpty(queryOrgId) && Guid.TryParse(queryOrgId, out var parsedOrgId))
        {
            orgId = parsedOrgId;
        }
        else
        {
            var claim = User.FindFirst("OrgId")?.Value ?? User.FindFirst("organization_id")?.Value;
            if (!string.IsNullOrEmpty(claim) && Guid.TryParse(claim, out var cOrgId))
            {
                orgId = cOrgId;
            }
        }

        // 2. Lookup user profile in TalentDbContext
        if (orgId == Guid.Empty)
        {
            var userId = _currentUserContext.GetUserId(ct);
            if (userId != Guid.Empty)
            {
                var profile = await _context.EmployeeProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.UserId == userId || e.Id == userId, ct);

                if (profile != null)
                {
                    orgId = profile.OrganizationId;
                }
            }
        }

        // 3. Fallback: Take first organization ID from EmployeeProfiles
        if (orgId == Guid.Empty)
        {
            orgId = await _context.EmployeeProfiles
                .AsNoTracking()
                .Select(e => e.OrganizationId)
                .FirstOrDefaultAsync(ct);
        }

        var result = await _mediator.Send(new GetEmployeesQuery(orgId), ct);

        await Send.OkAsync(result, ct);
    }
}