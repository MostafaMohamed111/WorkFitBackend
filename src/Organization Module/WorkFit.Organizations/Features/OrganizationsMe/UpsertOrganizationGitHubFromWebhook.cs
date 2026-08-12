using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Entities;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record UpsertOrganizationGitHubFromWebhookCommand(
    long GitHubOrganizationId,
    string GitHubOrganizationLogin,
    DateTimeOffset? GitHubCreatedAt,
    long GitHubInstallationId,
    DateTimeOffset InstalledAt) : IRequest<OrganizationDetailsResponse>;

public sealed class UpsertOrganizationGitHubFromWebhookCommandHandler
    : IRequestHandler<UpsertOrganizationGitHubFromWebhookCommand, OrganizationDetailsResponse>
{
    private readonly OrganizationDbContext _context;

    public UpsertOrganizationGitHubFromWebhookCommandHandler(OrganizationDbContext context) => _context = context;

    public async Task<OrganizationDetailsResponse> Handle(
        UpsertOrganizationGitHubFromWebhookCommand request,
        CancellationToken cancellationToken = default)
    {
        var organizationLogin = request.GitHubOrganizationLogin.Trim();
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(x => x.GitHubOrganizationId == request.GitHubOrganizationId, cancellationToken)
            ?? await _context.Organizations
                .FirstOrDefaultAsync(x => x.GitHubOrganizationLogin == organizationLogin, cancellationToken)
                ?? throw new OrganizationNotFoundException();

        organization.ConnectGitHubOrganization(
            request.GitHubOrganizationId,
            organizationLogin,
            request.GitHubCreatedAt);

        var installation = await _context.GitHubAppInstallations
            .FirstOrDefaultAsync(x => x.OrganizationId == organization.Id, cancellationToken);

        if (installation is null)
        {
            await _context.GitHubAppInstallations.AddAsync(
                GitHubAppInstallation.Create(
                    organization.Id,
                    request.GitHubInstallationId,
                    request.GitHubOrganizationId,
                    request.InstalledAt),
                cancellationToken);
        }
        else
        {
            installation.Update(
                request.GitHubInstallationId,
                request.GitHubOrganizationId,
                request.InstalledAt);
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
