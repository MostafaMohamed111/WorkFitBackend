using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployees;

public sealed class GetEmployeesHandler
    : IRequestHandler<GetEmployeesQuery, List<EmployeeListItemDto>>
{
    private readonly TalentDbContext _context;

    public GetEmployeesHandler(TalentDbContext context) => _context = context;

    public async Task<List<EmployeeListItemDto>> Handle(GetEmployeesQuery query, CancellationToken ct)
    {
        var dbQuery = _context.EmployeeProfiles
            .AsNoTracking()
            .Where(e => !e.IsDeleted);

        if (query.OrgId != Guid.Empty)
        {
            dbQuery = dbQuery.Where(e => e.OrganizationId == query.OrgId);
        }

        return await dbQuery
            .Select(e => new EmployeeListItemDto(
                e.Id, e.Name, e.Email ?? string.Empty, e.JobTitle,
                e.IsActive(), e.CurrentAllocationPercentage))
            .ToListAsync(ct);
    }
}