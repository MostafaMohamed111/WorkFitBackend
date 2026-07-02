using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.Teams;

public sealed record DeleteTeamRequest(Guid Id);
public sealed record DeleteTeamCommand(Guid Id) : IRequest;

public sealed class DeleteTeamCommandHandler : IRequestHandler<DeleteTeamCommand>
{
    private readonly OrganizationDbContext _context;

    public DeleteTeamCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task Handle(DeleteTeamCommand request, CancellationToken cancellationToken = default)
    {
        var team = await _context.Teams.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new TeamNotFoundException();

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteTeamEndpoint : Endpoint<DeleteTeamRequest>
{
    private readonly IMediator _mediator;

    public DeleteTeamEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/teams/{id}");
        AllowAnonymous();
        Options(x => x.WithTags("Organization"));
    }

    public override async Task HandleAsync(DeleteTeamRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteTeamCommand(req.Id), ct);
        await Send.NoContentAsync(ct);
    }
}
