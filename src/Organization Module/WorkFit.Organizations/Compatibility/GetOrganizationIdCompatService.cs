using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.TalentManagement.Contracts.LookUpServices;

namespace WorkFit.Organizations.Compatibility;

public sealed class GetOrganizationIdCompatService : IGetOrganizationIdCompatService
{
    private readonly OrganizationDbContext _orgDb;
    private readonly IEmployeeLookUpService _employeeLookUpService;

    public GetOrganizationIdCompatService(
        OrganizationDbContext orgDb,
        IEmployeeLookUpService employeeLookUpService)
    {
        _orgDb = orgDb;
        _employeeLookUpService = employeeLookUpService;
    }

    public async Task<Guid> GetOrganizationIdAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId != Guid.Empty)
        {
            var employee = await _employeeLookUpService.GetEmployeeByUserIdAsync(userId, ct);
            if (employee != null)
            {
                return employee.OrganizationId;
            }

            var org = await _orgDb.Organizations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId, ct);
            if (org != null)
            {
                return org.Id;
            }
        }

        var firstOrg = await _orgDb.Organizations.AsNoTracking().FirstOrDefaultAsync(ct);
        return firstOrg?.Id ?? Guid.Empty;
    }
}
