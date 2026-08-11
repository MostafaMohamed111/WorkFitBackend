using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record CreateOrganizationMeRequest(string OrganizationName);
public sealed record CreateOrganizationMeCommand(Guid UserId, string OrganizationName) : IRequest<OrganizationDetailsResponse>;

public sealed class CreateOrganizationMeCommandHandler : IRequestHandler<CreateOrganizationMeCommand, OrganizationDetailsResponse>
{
    private readonly OrganizationDbContext _context;
    private readonly ICreateOrganizationService _createOrganizationService;

    public CreateOrganizationMeCommandHandler(
        OrganizationDbContext context,
        ICreateOrganizationService createOrganizationService)
    {
        _context = context;
        _createOrganizationService = createOrganizationService;
    }

    public async Task<OrganizationDetailsResponse> Handle(CreateOrganizationMeCommand request, CancellationToken cancellationToken = default)
    {
        var existingOrganization = await _context.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        if (existingOrganization is not null)
            throw new EntityAlreadyExistsException(ModuleMarker.ModuleName, "Organization", existingOrganization.Id);

        var organizationId = await _createOrganizationService.CreateAsync(request.OrganizationName, request.UserId, cancellationToken);

        var organization = await _context.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == organizationId, cancellationToken)
            ?? throw new OrganizationNotFoundException();

        return new OrganizationDetailsResponse(
            organization.Id,
            organization.Name,
            organization.UserId,
            organization.BrandingJson,
            organization.SettingsJson,
            organization.GitHubOrganizationId,
            organization.GitHubOrganizationLogin,
            organization.GitHubCreatedAt,
            organization.CreatedAt,
            organization.UpdatedAt);
    }
}

public sealed class CreateOrganizationMeEndpoint : Endpoint<CreateOrganizationMeRequest, OrganizationDetailsResponse>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUserContext;

    public CreateOrganizationMeEndpoint(IMediator mediator, ICurrentUserContext currentUserContext)
    {
        _mediator = mediator;
        _currentUserContext = currentUserContext;
    }

    public override void Configure()
    {
        Post("/api/organizations/me");
        Roles("OrganizationOwner");
        Options(x => x.WithTags("Organization"));
    }

    public override async Task HandleAsync(CreateOrganizationMeRequest req, CancellationToken ct)
    {
        var userId = _currentUserContext.GetUserId(ct);
        var response = await _mediator.Send(new CreateOrganizationMeCommand(userId, req.OrganizationName), ct);
        await Send.OkAsync(response, ct);
    }
}
