
using WorkFit.Organizations.Domain.Entities;
using WorkFit.Organizations.Infrastructure.Data;
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
        var organization = Organization.Create(command.Name, command.UserId);
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync(cancellationToken);

        return organization.Id;
    }
}