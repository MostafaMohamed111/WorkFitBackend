using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record UpdateOrganizationSettingsRequest(Guid UserId, string SettingsJson);
public sealed record UpdateOrganizationSettingsCommand(Guid UserId, string SettingsJson) : IRequest<OrganizationSettingsResponse>;

public sealed class UpdateOrganizationSettingsCommandHandler : IRequestHandler<UpdateOrganizationSettingsCommand, OrganizationSettingsResponse>
{
    private readonly OrganizationDbContext _context;

    public UpdateOrganizationSettingsCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task<OrganizationSettingsResponse> Handle(UpdateOrganizationSettingsCommand request, CancellationToken cancellationToken = default)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken)
            ?? throw new OrganizationNotFoundException();

        organization.UpdateSettings(request.SettingsJson);
        await _context.SaveChangesAsync(cancellationToken);

        return new OrganizationSettingsResponse(
            organization.Id,
            organization.SettingsJson,
            organization.CreatedAt,
            organization.UpdatedAt);
    }
}

public sealed class UpdateOrganizationSettingsEndpoint : Endpoint<UpdateOrganizationSettingsRequest, OrganizationSettingsResponse>
{
    private readonly IMediator _mediator;

    public UpdateOrganizationSettingsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/organizations/me/settings");
        AllowAnonymous();
        Options(x => x.WithTags("Organization"));
    }

    public override async Task HandleAsync(UpdateOrganizationSettingsRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new UpdateOrganizationSettingsCommand(req.UserId, req.SettingsJson), ct);
        await Send.OkAsync(response, ct);
    }
}
