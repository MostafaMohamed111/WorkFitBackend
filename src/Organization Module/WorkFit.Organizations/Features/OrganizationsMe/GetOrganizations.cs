using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record GetOrganizationsQuery() : IRequest<List<OrganizationDetailsResponse>>;

public sealed class GetOrganizationsQueryHandler : IRequestHandler<GetOrganizationsQuery, List<OrganizationDetailsResponse>>
{
    private readonly OrganizationDbContext _context;

    public GetOrganizationsQueryHandler(OrganizationDbContext context) => _context = context;

    public async Task<List<OrganizationDetailsResponse>> Handle(GetOrganizationsQuery request, CancellationToken cancellationToken = default)
    {
        return await _context.Organizations
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(organization => new OrganizationDetailsResponse(
                organization.Id,
                organization.Name,
                organization.UserId,
                organization.BrandingJson,
                organization.SettingsJson,
                organization.GitHubOrganizationId,
                organization.GitHubOrganizationLogin,
                organization.GitHubCreatedAt,
                organization.CreatedAt,
                organization.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetOrganizationsEndpoint : EndpointWithoutRequest<List<OrganizationDetailsResponse>>
{
    private readonly IMediator _mediator;

    public GetOrganizationsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/organizations");
        AllowAnonymous();
        Options(x => x.WithTags("Organization"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var organizations = await _mediator.Send(new GetOrganizationsQuery(), ct);
        await Send.OkAsync(organizations, ct);
    }
}
