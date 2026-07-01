using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Entities;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Features.Teams;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.Departments;

public sealed record DepartmentResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record ListDepartmentsRequest(Guid OrganizationId);
public sealed record ListDepartmentsQuery(Guid OrganizationId) : IRequest<IReadOnlyList<DepartmentResponse>>;

public sealed class ListDepartmentsQueryHandler : IRequestHandler<ListDepartmentsQuery, IReadOnlyList<DepartmentResponse>>
{
    private readonly OrganizationDbContext _context;

    public ListDepartmentsQueryHandler(OrganizationDbContext context) => _context = context;

    public async Task<IReadOnlyList<DepartmentResponse>> Handle(ListDepartmentsQuery request, CancellationToken cancellationToken = default)
    {
        return await _context.Departments.AsNoTracking()
            .Where(x => x.OrganizationId == request.OrganizationId)
            .OrderBy(x => x.Name)
            .Select(x => new DepartmentResponse(x.Id, x.OrganizationId, x.Name, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }
}

public sealed class ListDepartmentsEndpoint : Endpoint<ListDepartmentsRequest, IReadOnlyList<DepartmentResponse>>
{
    private readonly IMediator _mediator;

    public ListDepartmentsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/departments");
        AllowAnonymous();
    }

    public override async Task HandleAsync(ListDepartmentsRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new ListDepartmentsQuery(req.OrganizationId), ct);
        await Send.OkAsync(response, ct);
    }
}

public sealed record GetDepartmentRequest(Guid Id);
public sealed record GetDepartmentQuery(Guid Id) : IRequest<DepartmentResponse>;

public sealed class GetDepartmentQueryHandler : IRequestHandler<GetDepartmentQuery, DepartmentResponse>
{
    private readonly OrganizationDbContext _context;

    public GetDepartmentQueryHandler(OrganizationDbContext context) => _context = context;

    public async Task<DepartmentResponse> Handle(GetDepartmentQuery request, CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new DepartmentNotFoundException();

        return new DepartmentResponse(department.Id, department.OrganizationId, department.Name, department.CreatedAt, department.UpdatedAt);
    }
}

public sealed class GetDepartmentEndpoint : Endpoint<GetDepartmentRequest, DepartmentResponse>
{
    private readonly IMediator _mediator;

    public GetDepartmentEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/departments/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetDepartmentRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetDepartmentQuery(req.Id), ct);
        await Send.OkAsync(response, ct);
    }
}

public sealed record CreateDepartmentRequest(Guid OrganizationId, string Name);
public sealed record CreateDepartmentCommand(Guid OrganizationId, string Name) : IRequest<DepartmentResponse>;

public sealed class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentResponse>
{
    private readonly OrganizationDbContext _context;

    public CreateDepartmentCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task<DepartmentResponse> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken = default)
    {
        var department = Department.Create(request.Name, request.OrganizationId);
        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);

        return new DepartmentResponse(department.Id, department.OrganizationId, department.Name, department.CreatedAt, department.UpdatedAt);
    }
}

public sealed class CreateDepartmentEndpoint : Endpoint<CreateDepartmentRequest, DepartmentResponse>
{
    private readonly IMediator _mediator;

    public CreateDepartmentEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/departments");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateDepartmentRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new CreateDepartmentCommand(req.OrganizationId, req.Name), ct);
        await Send.OkAsync(response, ct);
    }
}

public sealed record UpdateDepartmentRequest(Guid Id, string Name);
public sealed record UpdateDepartmentCommand(Guid Id, string Name) : IRequest<DepartmentResponse>;

public sealed class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentResponse>
{
    private readonly OrganizationDbContext _context;

    public UpdateDepartmentCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task<DepartmentResponse> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new DepartmentNotFoundException();

        department.UpdateName(request.Name);
        await _context.SaveChangesAsync(cancellationToken);

        return new DepartmentResponse(department.Id, department.OrganizationId, department.Name, department.CreatedAt, department.UpdatedAt);
    }
}

public sealed class UpdateDepartmentEndpoint : Endpoint<UpdateDepartmentRequest, DepartmentResponse>
{
    private readonly IMediator _mediator;

    public UpdateDepartmentEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/departments/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateDepartmentRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new UpdateDepartmentCommand(req.Id, req.Name), ct);
        await Send.OkAsync(response, ct);
    }
}

public sealed record DeleteDepartmentRequest(Guid Id);
public sealed record DeleteDepartmentCommand(Guid Id) : IRequest;

public sealed class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand>
{
    private readonly OrganizationDbContext _context;

    public DeleteDepartmentCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new DepartmentNotFoundException();

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteDepartmentEndpoint : Endpoint<DeleteDepartmentRequest>
{
    private readonly IMediator _mediator;

    public DeleteDepartmentEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/departments/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteDepartmentRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteDepartmentCommand(req.Id), ct);
        await Send.NoContentAsync(ct);
    }
}

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
    }

    public override async Task HandleAsync(GetDepartmentTeamsRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetDepartmentTeamsQuery(req.Id), ct);
        await Send.OkAsync(response, ct);
    }
}

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
    }

    public override async Task HandleAsync(CreateTeamRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new CreateTeamCommand(req.Id, req.Name), ct);
        await Send.OkAsync(response, ct);
    }
}
