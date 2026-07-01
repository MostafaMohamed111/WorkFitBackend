
using WorkFit.Organizations.Domain.Entities;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.CreateOrganization;

public sealed class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, Guid>
{
    private readonly OrganizationDbContext _context;

    public CreateOrganizationCommandHandler(OrganizationDbContext context)
    {
        _context = context;
    }
    public async Task<Guid> Handle(CreateOrganizationCommand command, CancellationToken cancellationToken = default)
    {
        var existingOrganization = await _context.Organizations.FindAsync(new object[] { command.Name }, cancellationToken);
            
        if(existingOrganization != null) 
            throw new EntityAlreadyExistsException(ModuleMarker.ModuleName, "Organization", existingOrganization.Id);

        var organization = Organization.Create(command.Name, command.UserId);
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync(cancellationToken);

        return organization.Id;
    }
}