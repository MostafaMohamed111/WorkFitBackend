using Microsoft.EntityFrameworkCore;
using WorkFit.TalentManagement.Contracts.WriteServices.CreateEmployee;
using WorkFit.TalentManagement.Domain.Entities;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.CrossCutting;

internal sealed class GetOrCreateExternalEmployeeService : IGetOrCreateExternalEmployeeService
{
    private readonly TalentDbContext _db;

    public GetOrCreateExternalEmployeeService(TalentDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> GetOrCreateAsync(
        Guid organizationId, 
        Guid userId,
        string sourceSystem, 
        string externalAccountId, 
        string externalDisplayName, 
        string? email, 
        string jobTitle, 
        CancellationToken cancellationToken = default)
    {
        var existingMapping = await _db.IdentityMappings
            .FirstOrDefaultAsync(m => m.SourceSystem == sourceSystem && m.ExternalAccountId == externalAccountId, cancellationToken);

        if (existingMapping != null)
        {
            return existingMapping.EmployeeProfileId;
        }

        
        var existingEmployee = await _db.EmployeeProfiles
            .Include(e => e.IdentityMappings)
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

        if (existingEmployee != null)
        {
            existingEmployee.AddExternalIdentity(sourceSystem, externalAccountId, externalDisplayName);
            await _db.SaveChangesAsync(cancellationToken);
            return existingEmployee.Id;
        }

        
        var employee = EmployeeProfile.Create(organizationId, userId, email, externalDisplayName, jobTitle);
        employee.AddExternalIdentity(sourceSystem, externalAccountId, externalDisplayName);
        
        await _db.EmployeeProfiles.AddAsync(employee, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return employee.Id;
    }
}
