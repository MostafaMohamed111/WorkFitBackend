using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.Teams;

public sealed record GetTeamRequest(Guid Id);
public sealed record GetTeamQuery(Guid Id) : IRequest<TeamResponse>;

public sealed class GetTeamQueryHandler : IRequestHandler<GetTeamQuery, TeamResponse>
{
    private readonly OrganizationDbContext _context;

    public GetTeamQueryHandler(OrganizationDbContext context) => _context = context;

    public async Task<TeamResponse> Handle(GetTeamQuery request, CancellationToken cancellationToken = default)
    {
        var team = await _context.Teams.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new TeamNotFoundException();

        return new TeamResponse(team.Id, team.DepartmentId, team.Name, team.LeadUserId, team.CreatedAt, team.UpdatedAt);
    }
}

public sealed class GetTeamEndpoint : Endpoint<GetTeamRequest, TeamResponse>
{
    private readonly IMediator _mediator;

    public GetTeamEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/teams/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetTeamRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetTeamQuery(req.Id), ct);
        await Send.OkAsync(response, ct);
    }
}
