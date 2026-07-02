using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.Teams;

public sealed record UpdateTeamRequest(Guid Id, string Name);
public sealed record UpdateTeamCommand(Guid Id, string Name) : IRequest<TeamResponse>;

public sealed class UpdateTeamCommandHandler : IRequestHandler<UpdateTeamCommand, TeamResponse>
{
    private readonly OrganizationDbContext _context;

    public UpdateTeamCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task<TeamResponse> Handle(UpdateTeamCommand request, CancellationToken cancellationToken = default)
    {
        var team = await _context.Teams.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new TeamNotFoundException();

        team.UpdateName(request.Name);
        await _context.SaveChangesAsync(cancellationToken);

        return new TeamResponse(team.Id, team.DepartmentId, team.Name, team.LeadUserId, team.CreatedAt, team.UpdatedAt);
    }
}

public sealed class UpdateTeamEndpoint : Endpoint<UpdateTeamRequest, TeamResponse>
{
    private readonly IMediator _mediator;

    public UpdateTeamEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/teams/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateTeamRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new UpdateTeamCommand(req.Id, req.Name), ct);
        await Send.OkAsync(response, ct);
    }
}
