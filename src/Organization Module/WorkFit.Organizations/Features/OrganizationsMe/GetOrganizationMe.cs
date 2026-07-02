using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record GetOrganizationMeRequest(Guid UserId);
public sealed record GetOrganizationMeQuery(Guid UserId) : IRequest<OrganizationDetailsResponse>;

public sealed class GetOrganizationMeQueryHandler : IRequestHandler<GetOrganizationMeQuery, OrganizationDetailsResponse>
{
    private readonly OrganizationDbContext _context;

    public GetOrganizationMeQueryHandler(OrganizationDbContext context) => _context = context;

    public async Task<OrganizationDetailsResponse> Handle(GetOrganizationMeQuery request, CancellationToken cancellationToken = default)
    {
        var organization = await _context.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken)
            ?? throw new OrganizationNotFoundException();

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

public sealed class GetOrganizationMeEndpoint : Endpoint<GetOrganizationMeRequest, OrganizationDetailsResponse>
{
    private readonly IMediator _mediator;

    public GetOrganizationMeEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/organizations/me");
        AllowAnonymous();
        Options(x => x.WithTags("Organization"));
    }

    public override async Task HandleAsync(GetOrganizationMeRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetOrganizationMeQuery(req.UserId), ct);
        await Send.OkAsync(response, ct);
    }
}
