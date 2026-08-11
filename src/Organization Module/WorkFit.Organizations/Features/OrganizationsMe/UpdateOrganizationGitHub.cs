using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Entities;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record UpdateOrganizationGitHubRequest(
    Guid UserId,
    long GitHubOrganizationId,
    string GitHubOrganizationLogin,
    DateTimeOffset? GitHubCreatedAt,
    long GitHubInstallationId,
    DateTimeOffset? InstalledAt);

public sealed record UpdateOrganizationGitHubCommand(
    Guid UserId,
    long GitHubOrganizationId,
    string GitHubOrganizationLogin,
    DateTimeOffset? GitHubCreatedAt,
    long GitHubInstallationId,
    DateTimeOffset? InstalledAt) : IRequest<OrganizationDetailsResponse>;

public sealed class UpdateOrganizationGitHubCommandHandler : IRequestHandler<UpdateOrganizationGitHubCommand, OrganizationDetailsResponse>
{
    private readonly OrganizationDbContext _context;

    public UpdateOrganizationGitHubCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task<OrganizationDetailsResponse> Handle(UpdateOrganizationGitHubCommand request, CancellationToken cancellationToken = default)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken)
            ?? throw new OrganizationNotFoundException();

        organization.ConnectGitHubOrganization(request.GitHubOrganizationId, request.GitHubOrganizationLogin, request.GitHubCreatedAt);

        var installation = await _context.GitHubAppInstallations
            .FirstOrDefaultAsync(x => x.OrganizationId == organization.Id, cancellationToken);

        var installedAt = request.InstalledAt ?? DateTimeOffset.UtcNow;
        if (installation is null)
        {
            await _context.GitHubAppInstallations.AddAsync(
                GitHubAppInstallation.Create(
                    organization.Id,
                    request.GitHubInstallationId,
                    request.GitHubOrganizationId,
                    installedAt),
                cancellationToken);
        }
        else
        {
            installation.Update(request.GitHubInstallationId, request.GitHubOrganizationId, installedAt);
        }

        await _context.SaveChangesAsync(cancellationToken);

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

public sealed class UpdateOrganizationGitHubEndpoint : Endpoint<UpdateOrganizationGitHubRequest, OrganizationDetailsResponse>
{
    private readonly IMediator _mediator;

    public UpdateOrganizationGitHubEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/organizations/me/github");
        AllowAnonymous();
        Options(x => x.WithTags("Organization"));
    }

    public override async Task HandleAsync(UpdateOrganizationGitHubRequest req, CancellationToken ct)
    {
        var response = await _mediator.Send(
            new UpdateOrganizationGitHubCommand(
                req.UserId,
                req.GitHubOrganizationId,
                req.GitHubOrganizationLogin,
                req.GitHubCreatedAt,
                req.GitHubInstallationId,
                req.InstalledAt),
            ct);

        await Send.OkAsync(response, ct);
    }
}
