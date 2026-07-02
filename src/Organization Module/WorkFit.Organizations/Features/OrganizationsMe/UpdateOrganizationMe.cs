using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record UpdateOrganizationMeRequest(Guid UserId, string Name, string BrandingJson, string SettingsJson);
public sealed record UpdateOrganizationMeCommand(Guid UserId, string Name, string BrandingJson, string SettingsJson) : IRequest<OrganizationDetailsResponse>;

public sealed class UpdateOrganizationMeCommandHandler : IRequestHandler<UpdateOrganizationMeCommand, OrganizationDetailsResponse>
{
    private readonly OrganizationDbContext _context;

    public UpdateOrganizationMeCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task<OrganizationDetailsResponse> Handle(UpdateOrganizationMeCommand request, CancellationToken cancellationToken = default)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken)
            ?? throw new OrganizationNotFoundException();

        organization.UpdateDetails(request.Name, request.BrandingJson, request.SettingsJson);
        await _context.SaveChangesAsync(cancellationToken);

        return new OrganizationDetailsResponse(
            organization.Id,
            organization.Name,
            organization.UserId,
            organization.BrandingJson,
            organization.SettingsJson,
            organization.CreatedAt,
            organization.UpdatedAt);
    }
}

public sealed class UpdateOrganizationMeEndpoint : Endpoint<UpdateOrganizationMeRequest, OrganizationDetailsResponse>
{
    private readonly IMediator _mediator;

    public UpdateOrganizationMeEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/organizations/me");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateOrganizationMeRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new UpdateOrganizationMeCommand(req.UserId, req.Name, req.BrandingJson, req.SettingsJson), ct);
        await Send.OkAsync(response, ct);
    }
}
