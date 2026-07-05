
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.Organizations.Domain.Entities;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;

namespace WorkFit.Organizations.CrossModule.CreateOrganization;

public sealed class CreateOrganizationCommandHandler : ICreateOrganizationService
{
    private readonly OrganizationDbContext _context;

    public CreateOrganizationCommandHandler(OrganizationDbContext context)
    {
        _context = context;
    }
    public async Task<Guid> CreateAsync(string organizationName, Guid userId, CancellationToken cancellationToken = default)
    {
        var existingOrganization = await _context.Organizations.FirstOrDefaultAsync(o => o.Name == organizationName, cancellationToken);
            
        if(existingOrganization != null) 
            throw new EntityAlreadyExistsException(ModuleMarker.ModuleName, "Organization", existingOrganization.Id);

        var organization = Organization.Create(organizationName, userId);
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync(cancellationToken);

        return organization.Id;
    }
}