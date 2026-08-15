using Microsoft.EntityFrameworkCore;
using WorkFit.TalentManagement.Contracts.WriteServices.CreateEmployee;
using WorkFit.TalentManagement.Domain.Entities;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.CrossCutting;

internal sealed class GetOrCreateExternalEmployeeService : IGetOrCreateExternalEmployeeService
{
    private readonly TalentDbContext _db;
    private readonly EmployeeIndexingStatePublisher _publisher;

    public GetOrCreateExternalEmployeeService(TalentDbContext db, EmployeeIndexingStatePublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<ExternalEmployeeResolution> GetOrCreateAsync(
        Guid organizationId, 
        string sourceSystem, 
        string externalAccountId, 
        string externalDisplayName, 
        string? email, 
        string jobTitle, 
        string? linkedInUrl = null,
        CancellationToken cancellationToken = default)
    {
        var existingMapping = await _db.IdentityMappings
            .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.SourceSystem == sourceSystem && m.ExternalAccountId == externalAccountId, cancellationToken);

        if (existingMapping != null)
        {
            var mappedEmployee = await _db.EmployeeProfiles.FirstOrDefaultAsync(e => e.Id == existingMapping.EmployeeProfileId, cancellationToken);
            if (mappedEmployee != null && !string.IsNullOrWhiteSpace(linkedInUrl) && string.IsNullOrWhiteSpace(mappedEmployee.LinkedInUrl))
            {
                mappedEmployee.UpdateEmployeePersonalData(mappedEmployee.Name, mappedEmployee.JobTitle, mappedEmployee.Bio, linkedInUrl);
                await _db.SaveChangesAsync(cancellationToken);
                await _publisher.PublishAsync(mappedEmployee.Id, "Updated", cancellationToken);
            }
            return new(existingMapping.EmployeeProfileId, mappedEmployee?.UserId == Guid.Empty, mappedEmployee?.UserId != Guid.Empty);
        }

        var employee = EmployeeProfile.Create(organizationId, Guid.Empty, email, externalDisplayName, jobTitle, linkedInUrl: linkedInUrl);
        employee.AddExternalIdentity(sourceSystem, externalAccountId, externalDisplayName);
        
        await _db.EmployeeProfiles.AddAsync(employee, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await _publisher.PublishAsync(employee.Id, "Created", cancellationToken);

        return new(employee.Id, true, false);
    }
}
