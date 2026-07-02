using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record GetOrganizationSettingsRequest(Guid UserId);
public sealed record GetOrganizationSettingsQuery(Guid UserId) : IRequest<OrganizationSettingsResponse>;

public sealed class GetOrganizationSettingsQueryHandler : IRequestHandler<GetOrganizationSettingsQuery, OrganizationSettingsResponse>
{
    private readonly OrganizationDbContext _context;

    public GetOrganizationSettingsQueryHandler(OrganizationDbContext context) => _context = context;

    public async Task<OrganizationSettingsResponse> Handle(GetOrganizationSettingsQuery request, CancellationToken cancellationToken = default)
    {
        var organization = await _context.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken)
            ?? throw new OrganizationNotFoundException();

        return new OrganizationSettingsResponse(
            organization.Id,
            organization.SettingsJson,
            organization.CreatedAt,
            organization.UpdatedAt);
    }
}

public sealed class GetOrganizationSettingsEndpoint : Endpoint<GetOrganizationSettingsRequest, OrganizationSettingsResponse>
{
    private readonly IMediator _mediator;

    public GetOrganizationSettingsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/organizations/me/settings");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetOrganizationSettingsRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetOrganizationSettingsQuery(req.UserId), ct);
        await Send.OkAsync(response, ct);
    }
}
