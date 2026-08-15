using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployees;

public sealed class GetEmployeesEndpoint
    : EndpointWithoutRequest<List<EmployeeListItemDto>>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IGetOrganizationIdService _organizations;
    private readonly TalentDbContext _context;

    public GetEmployeesEndpoint(
        IMediator mediator,
        ICurrentUserContext currentUserContext,
        IGetOrganizationIdService organizations,
        TalentDbContext context)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
        _organizations = organizations;
        _context = context;
    }

    public override void Configure()
    {
        Get("/api/employees/{orgId?}", "/api/employees");
        Roles("TeamLeader", "OrganizationOwner", "Admin", "SuperAdmin");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var orgIdRoute = Route<string>("orgId", isRequired: false);
        Guid orgId = Guid.Empty;
        if (!string.IsNullOrEmpty(orgIdRoute) && Guid.TryParse(orgIdRoute, out var parsedOrgId))
        {
            orgId = parsedOrgId;
        }

        if (orgId == Guid.Empty)
        {
            var userId = _currentUserContext.GetUserId(ct);
            try
            {
                orgId = await _organizations.GetOrganizationIdAsync(userId, ct);
            }
            catch
            {
                var employee = await _context.EmployeeProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => (e.UserId == userId || e.Id == userId) && !e.IsDeleted, ct);
                if (employee != null)
                {
                    orgId = employee.OrganizationId;
                }
            }
        }

        var result = await _mediator.Send(new GetEmployeesQuery(orgId), ct);

        await Send.OkAsync(result, ct);
    }
}