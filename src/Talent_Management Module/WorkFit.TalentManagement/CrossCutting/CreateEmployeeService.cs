using WorkFit.TalentManagement.Contracts.WriteServices.CreateEmployee;
using WorkFit.TalentManagement.Infrastructure.Data;
using WorkFit.TalentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WorkFit.TalentManagement.CrossCutting;

internal sealed class CreateEmployeeService : ICreateEmployeeService
{
    private readonly TalentDbContext _db;

    public CreateEmployeeService(TalentDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> CreateEmployeeAsync(EmployeeDetails details, CancellationToken cancellationToken = default)
    {
        var existing = await _db.EmployeeProfiles
            .FirstOrDefaultAsync(e => e.UserId == details.userId || (!string.IsNullOrEmpty(details.email) && e.Email == details.email), cancellationToken);
            
        if (existing != null)
        {
            return existing.Id;
        }

        var employee = EmployeeProfile.Create(details.organizationId, details.userId, details.email, details.name, details.jobTitle);
        _db.EmployeeProfiles.Add(employee);
        await _db.SaveChangesAsync(cancellationToken);
        
        return employee.Id;
    }
}
