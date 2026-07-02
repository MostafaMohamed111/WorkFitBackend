using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Organizations.Domain.Entities;
using WorkFit.Organizations.Features.Teams;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.Departments;

public sealed record CreateTeamRequest(Guid Id, string Name);
public sealed record CreateTeamCommand(Guid DepartmentId, string Name) : IRequest<TeamResponse>;

public sealed class CreateTeamCommandHandler : IRequestHandler<CreateTeamCommand, TeamResponse>
{
    private readonly OrganizationDbContext _context;

    public CreateTeamCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task<TeamResponse> Handle(CreateTeamCommand request, CancellationToken cancellationToken = default)
    {
        var team = Team.Create(request.Name, request.DepartmentId);
        _context.Teams.Add(team);
        await _context.SaveChangesAsync(cancellationToken);

        return new TeamResponse(team.Id, team.DepartmentId, team.Name, team.LeadUserId, team.CreatedAt, team.UpdatedAt);
    }
}

public sealed class CreateTeamEndpoint : Endpoint<CreateTeamRequest, TeamResponse>
{
    private readonly IMediator _mediator;

    public CreateTeamEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/departments/{id}/teams");
        AllowAnonymous();
        Options(x => x.WithTags("Organization"));
    }

    public override async Task HandleAsync(CreateTeamRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new CreateTeamCommand(req.Id, req.Name), ct);
        await Send.OkAsync(response, ct);
    }
}
