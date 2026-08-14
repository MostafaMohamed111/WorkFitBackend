using WorkFit.TalentManagement.Contracts.WriteServices.CreateEmployee;
using WorkFit.TalentManagement.Infrastructure.Data;
using WorkFit.TalentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace WorkFit.TalentManagement.CrossCutting;

internal sealed class CreateEmployeeService : ICreateEmployeeService
{
    private readonly TalentDbContext _db;
    private readonly EmployeeIndexingStatePublisher _publisher;

    public CreateEmployeeService(TalentDbContext db, EmployeeIndexingStatePublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<Guid> CreateEmployeeAsync(EmployeeDetails details, CancellationToken cancellationToken = default)
    {
        var existing = await _db.EmployeeProfiles
            .FirstOrDefaultAsync(e => e.UserId == details.userId || (!string.IsNullOrEmpty(details.email) && e.Email == details.email), cancellationToken);
            
        if (existing != null)
        {
            return existing.Id;
        }

        var employee = EmployeeProfile.Create(
            details.organizationId,
            details.userId,
            details.email,
            details.name,
            details.jobTitle,
            details.hireDate);
        _db.EmployeeProfiles.Add(employee);
        await _db.SaveChangesAsync(cancellationToken);
        await _publisher.PublishAsync(employee.Id, "Created", cancellationToken);
        
        return employee.Id;
    }
}
