using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Features.Teams;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.Departments;

public sealed record GetDepartmentTeamsRequest(Guid Id);
public sealed record GetDepartmentTeamsQuery(Guid Id) : IRequest<IReadOnlyList<TeamResponse>>;

public sealed class GetDepartmentTeamsQueryHandler : IRequestHandler<GetDepartmentTeamsQuery, IReadOnlyList<TeamResponse>>
{
    private readonly OrganizationDbContext _context;

    public GetDepartmentTeamsQueryHandler(OrganizationDbContext context) => _context = context;

    public async Task<IReadOnlyList<TeamResponse>> Handle(GetDepartmentTeamsQuery request, CancellationToken cancellationToken = default)
    {
        return await _context.Teams.AsNoTracking()
            .Where(x => x.DepartmentId == request.Id)
            .OrderBy(x => x.Name)
            .Select(x => new TeamResponse(x.Id, x.DepartmentId, x.Name, x.LeadUserId, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetDepartmentTeamsEndpoint : Endpoint<GetDepartmentTeamsRequest, IReadOnlyList<TeamResponse>>
{
    private readonly IMediator _mediator;

    public GetDepartmentTeamsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/departments/{id}/teams");
        AllowAnonymous();
        Options(x => x.WithTags("Organization"));
    }

    public override async Task HandleAsync(GetDepartmentTeamsRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetDepartmentTeamsQuery(req.Id), ct);
        await Send.OkAsync(response, ct);
    }
}
