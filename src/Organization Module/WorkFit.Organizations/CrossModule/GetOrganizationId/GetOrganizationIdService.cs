using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.TalentManagement.Contracts.LookUpServices;

namespace WorkFit.Organizations.CrossModule.GetOrganizationId;

public sealed class GetOrganizationIdService : IGetOrganizationIdService
{
    private readonly OrganizationDbContext _context;
    private readonly IEmployeeLookUpService _employeeLookUpService;

    public GetOrganizationIdService(
        OrganizationDbContext context,
        IEmployeeLookUpService employeeLookUpService)
    {
        _context = context;
        _employeeLookUpService = employeeLookUpService;
    }

    public async Task<Guid> GetOrganizationIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeLookUpService.GetEmployeeByUserIdAsync(userId, cancellationToken);
        if (employee is not null)
        {
            return employee.OrganizationId;
        }

        var organization = await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            ?? throw new OrganizationNotFoundException();

        return organization.Id;
    }
}
