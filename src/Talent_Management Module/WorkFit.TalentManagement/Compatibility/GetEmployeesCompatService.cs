using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.TalentManagement.Features.Employee.GetEmployees;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Compatibility;

public sealed class GetEmployeesCompatService : IGetEmployeesCompatService
{
    private readonly TalentDbContext _context;
    private readonly IGetOrganizationIdService _getOrganizationIdService;

    public GetEmployeesCompatService(
        TalentDbContext context,
        IGetOrganizationIdService getOrganizationIdService)
    {
        _context = context;
        _getOrganizationIdService = getOrganizationIdService;
    }

    public async Task<List<EmployeeListItemDto>> GetEmployeesAsync(Guid userId, CancellationToken ct = default)
    {
        Guid orgId = Guid.Empty;
        if (userId != Guid.Empty)
        {
            try
            {
                orgId = await _getOrganizationIdService.GetOrganizationIdAsync(userId, ct);
            }
            catch
            {
                var emp = await _context.EmployeeProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => (e.UserId == userId || e.Id == userId) && !e.IsDeleted, ct);
                if (emp != null) orgId = emp.OrganizationId;
            }
        }

        if (orgId == Guid.Empty)
        {
            return new List<EmployeeListItemDto>();
        }

        return await _context.EmployeeProfiles
            .AsNoTracking()
            .Where(e => !e.IsDeleted && e.OrganizationId == orgId)
            .OrderBy(e => e.Name)
            .Select(e => new EmployeeListItemDto(
                e.Id,
                e.Name,
                e.Email ?? string.Empty,
                e.JobTitle,
                e.Status == Domain.Enums.EmployeeProfileStatus.Active,
                e.CurrentAllocationPercentage))
            .ToListAsync(ct);
    }
}
