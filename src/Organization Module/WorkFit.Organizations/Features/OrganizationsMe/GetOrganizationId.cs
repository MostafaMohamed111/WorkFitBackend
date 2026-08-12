using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record GetOrganizationIdRequest(Guid UserId);
public sealed record GetOrganizationIdQuery(Guid UserId) : IRequest<Guid>;

public sealed class GetOrganizationIdQueryHandler : IRequestHandler<GetOrganizationIdQuery, Guid>
{
    private readonly OrganizationDbContext _context;

    public GetOrganizationIdQueryHandler(OrganizationDbContext context) => _context = context;

    public async Task<Guid> Handle(GetOrganizationIdQuery request, CancellationToken cancellationToken = default)
    {
        var organization = await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken)
            ?? throw new OrganizationNotFoundException();

        return organization.Id;
    }
}

public sealed class GetOrganizationIdEndpoint : Endpoint<GetOrganizationIdRequest, Guid>
{
    private readonly IMediator _mediator;

    public GetOrganizationIdEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/organizations/me/id");
        AllowAnonymous();
        Options(x => x.WithTags("Organization"));
    }

    public override async Task HandleAsync(GetOrganizationIdRequest req, CancellationToken ct)
    {
        var organizationId = await _mediator.Send(new GetOrganizationIdQuery(req.UserId), ct);
        await Send.OkAsync(organizationId, ct);
    }
}
