using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Entities;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.Teams;

public sealed record TeamResponse(
    Guid Id,
    Guid DepartmentId,
    string Name,
    Guid? LeadUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

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
    }

    public override async Task HandleAsync(DeleteTeamRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteTeamCommand(req.Id), ct);
        await Send.NoContentAsync(ct);
    }
}
