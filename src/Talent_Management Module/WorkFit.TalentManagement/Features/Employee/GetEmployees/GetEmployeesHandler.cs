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
        return await _context.Employees
            .Include(e => e.Profile)
            .Where(e => e.OrganizationId == query.OrgId && e.IsActive)
            .OrderBy(e => e.LastName)
            .Select(e => new EmployeeListItemDto(
                e.Id, e.FirstName, e.LastName, e.Email, e.JobTitle,
                e.IsActive, e.Profile.AvailabilityPercentage))
            .ToListAsync(ct);
    }
}