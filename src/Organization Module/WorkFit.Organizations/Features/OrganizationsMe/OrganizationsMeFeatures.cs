using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Entities;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record OrganizationDetailsResponse(
    Guid Id,
    string Name,
    Guid UserId,
    string BrandingJson,
    string SettingsJson,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record OrganizationSettingsResponse(
    Guid Id,
    string SettingsJson,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record GetOrganizationMeRequest(Guid UserId);
public sealed record GetOrganizationMeQuery(Guid UserId) : IRequest<OrganizationDetailsResponse>;

public sealed class GetOrganizationMeQueryHandler : IRequestHandler<GetOrganizationMeQuery, OrganizationDetailsResponse>
{
    private readonly OrganizationDbContext _context;

    public GetOrganizationMeQueryHandler(OrganizationDbContext context) => _context = context;

    public async Task<OrganizationDetailsResponse> Handle(GetOrganizationMeQuery request, CancellationToken cancellationToken = default)
    {
        var organization = await _context.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken)
            ?? throw new OrganizationNotFoundException();

        return new OrganizationDetailsResponse(
            organization.Id,
            organization.Name,
            organization.UserId,
            organization.BrandingJson,
            organization.SettingsJson,
            organization.CreatedAt,
            organization.UpdatedAt);
    }
}

public sealed class GetOrganizationMeEndpoint : Endpoint<GetOrganizationMeRequest, OrganizationDetailsResponse>
{
    private readonly IMediator _mediator;

    public GetOrganizationMeEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/organizations/me");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetOrganizationMeRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetOrganizationMeQuery(req.UserId), ct);
        await Send.OkAsync(response, ct);
    }
}

public sealed record UpdateOrganizationMeRequest(Guid UserId, string Name, string BrandingJson, string SettingsJson);
public sealed record UpdateOrganizationMeCommand(Guid UserId, string Name, string BrandingJson, string SettingsJson) : IRequest<OrganizationDetailsResponse>;

public sealed class UpdateOrganizationMeCommandHandler : IRequestHandler<UpdateOrganizationMeCommand, OrganizationDetailsResponse>
{
    private readonly OrganizationDbContext _context;

    public UpdateOrganizationMeCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task<OrganizationDetailsResponse> Handle(UpdateOrganizationMeCommand request, CancellationToken cancellationToken = default)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken)
            ?? throw new OrganizationNotFoundException();

        organization.UpdateDetails(request.Name, request.BrandingJson, request.SettingsJson);
        await _context.SaveChangesAsync(cancellationToken);

        return new OrganizationDetailsResponse(
            organization.Id,
            organization.Name,
            organization.UserId,
            organization.BrandingJson,
            organization.SettingsJson,
            organization.CreatedAt,
            organization.UpdatedAt);
    }
}

public sealed class UpdateOrganizationMeEndpoint : Endpoint<UpdateOrganizationMeRequest, OrganizationDetailsResponse>
{
    private readonly IMediator _mediator;

    public UpdateOrganizationMeEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/organizations/me");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateOrganizationMeRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new UpdateOrganizationMeCommand(req.UserId, req.Name, req.BrandingJson, req.SettingsJson), ct);
        await Send.OkAsync(response, ct);
    }
}

public sealed record GetOrganizationSettingsRequest(Guid UserId);
public sealed record GetOrganizationSettingsQuery(Guid UserId) : IRequest<OrganizationSettingsResponse>;

public sealed class GetOrganizationSettingsQueryHandler : IRequestHandler<GetOrganizationSettingsQuery, OrganizationSettingsResponse>
{
    private readonly OrganizationDbContext _context;

    public GetOrganizationSettingsQueryHandler(OrganizationDbContext context) => _context = context;

    public async Task<OrganizationSettingsResponse> Handle(GetOrganizationSettingsQuery request, CancellationToken cancellationToken = default)
    {
        var organization = await _context.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken)
            ?? throw new OrganizationNotFoundException();

        return new OrganizationSettingsResponse(
            organization.Id,
            organization.SettingsJson,
            organization.CreatedAt,
            organization.UpdatedAt);
    }
}

public sealed class GetOrganizationSettingsEndpoint : Endpoint<GetOrganizationSettingsRequest, OrganizationSettingsResponse>
{
    private readonly IMediator _mediator;

    public GetOrganizationSettingsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/organizations/me/settings");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetOrganizationSettingsRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new GetOrganizationSettingsQuery(req.UserId), ct);
        await Send.OkAsync(response, ct);
    }
}

public sealed record UpdateOrganizationSettingsRequest(Guid UserId, string SettingsJson);
public sealed record UpdateOrganizationSettingsCommand(Guid UserId, string SettingsJson) : IRequest<OrganizationSettingsResponse>;

public sealed class UpdateOrganizationSettingsCommandHandler : IRequestHandler<UpdateOrganizationSettingsCommand, OrganizationSettingsResponse>
{
    private readonly OrganizationDbContext _context;

    public UpdateOrganizationSettingsCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task<OrganizationSettingsResponse> Handle(UpdateOrganizationSettingsCommand request, CancellationToken cancellationToken = default)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken)
            ?? throw new OrganizationNotFoundException();

        organization.UpdateSettings(request.SettingsJson);
        await _context.SaveChangesAsync(cancellationToken);

        return new OrganizationSettingsResponse(
            organization.Id,
            organization.SettingsJson,
            organization.CreatedAt,
            organization.UpdatedAt);
    }
}

public sealed class UpdateOrganizationSettingsEndpoint : Endpoint<UpdateOrganizationSettingsRequest, OrganizationSettingsResponse>
{
    private readonly IMediator _mediator;

    public UpdateOrganizationSettingsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/organizations/me/settings");
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateOrganizationSettingsRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(new UpdateOrganizationSettingsCommand(req.UserId, req.SettingsJson), ct);
        await Send.OkAsync(response, ct);
    }
}
