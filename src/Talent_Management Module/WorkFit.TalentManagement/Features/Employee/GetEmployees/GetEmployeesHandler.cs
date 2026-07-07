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
        return await _context.EmployeeProfiles
            .Where(e => e.OrganizationId == query.OrgId && !e.IsDeleted)
            .Select(e => new EmployeeListItemDto(
                e.Id, e.Name, e.Email, e.JobTitle,
                e.IsActive(), e.CurrentAllocationPercentage))
            .ToListAsync(ct);
    }
}