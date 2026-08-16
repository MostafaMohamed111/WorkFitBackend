using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployees;

public sealed class GetEmployeesHandler
    : IRequestHandler<GetEmployeesQuery, List<EmployeeListItemDto>>
{
    private readonly TalentDbContext _context;

    private readonly IGetOrganizationIdService _getOrganizationIdService;
    private readonly ICurrentUserContext _currentUserContext;

    public GetEmployeesHandler(TalentDbContext context,
        IGetOrganizationIdService getOrganizationIdService,
        ICurrentUserContext currentUserContext)
    {
        _context = context;
        _getOrganizationIdService = getOrganizationIdService;
        _currentUserContext = currentUserContext;
    }

    public async Task<List<EmployeeListItemDto>> Handle(GetEmployeesQuery query, CancellationToken ct)
    {
        var userId = _currentUserContext.GetUserId();
        var orgId = await _getOrganizationIdService.GetOrganizationIdAsync(userId, ct);
        var dbQuery = _context.EmployeeProfiles
            .AsNoTracking()
            .Where(e => !e.IsDeleted && e.OrganizationId == orgId);


        return await dbQuery
            .Select(e => new EmployeeListItemDto(
                e.Id, e.Name, e.Email ?? string.Empty, e.JobTitle,
                e.IsActive(), e.CurrentAllocationPercentage))
            .ToListAsync(ct);
    }
}