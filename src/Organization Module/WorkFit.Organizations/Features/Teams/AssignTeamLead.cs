using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.Teams;

public sealed record AssignTeamLeadRequest(Guid Id, Guid? LeadUserId);
public sealed record AssignTeamLeadCommand(Guid Id, Guid? LeadUserId) : IRequest<TeamResponse>;

public sealed class AssignTeamLeadCommandHandler : IRequestHandler<AssignTeamLeadCommand, TeamResponse>
{
    private readonly OrganizationDbContext _context;

    public AssignTeamLeadCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task<TeamResponse> Handle(AssignTeamLeadCommand request, CancellationToken cancellationToken = default)
    {
        var team = await _context.Teams.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new TeamNotFoundException();

        team.AssignLead(request.LeadUserId);
        await _context.SaveChangesAsync(cancellationToken);

        return new TeamResponse(team.Id, team.DepartmentId, team.Name, team.LeadUserId, team.CreatedAt, team.UpdatedAt);
    }
}

public sealed class AssignTeamLeadEndpoint : Endpoint<AssignTeamLeadRequest, TeamResponse>
{
    private readonly IMediator _mediator;

    public AssignTeamLeadEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/teams/{id}/lead");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AssignTeamLeadRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new AssignTeamLeadCommand(req.Id, req.LeadUserId), ct);
        await Send.OkAsync(response, ct);
    }
}
