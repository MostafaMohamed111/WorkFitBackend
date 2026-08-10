using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record GetOrganizationGitHubInfoQuery(Guid OrganizationId) : IRequest<OrganizationGitHubInfoResponse>;

public sealed record OrganizationGitHubInfoResponse(
    Guid OrganizationId,
    long? GitHubOrganizationId,
    string? GitHubOrganizationLogin,
    DateTimeOffset? GitHubCreatedAt);

public sealed class GetOrganizationGitHubInfoQueryHandler : IRequestHandler<GetOrganizationGitHubInfoQuery, OrganizationGitHubInfoResponse>
{
    private readonly OrganizationDbContext _context;

    public GetOrganizationGitHubInfoQueryHandler(OrganizationDbContext context) => _context = context;

    public async Task<OrganizationGitHubInfoResponse> Handle(GetOrganizationGitHubInfoQuery request, CancellationToken cancellationToken = default)
    {
        var organization = await _context.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.OrganizationId, cancellationToken)
            ?? throw new InvalidOperationException($"Organization '{request.OrganizationId}' was not found.");

        return new OrganizationGitHubInfoResponse(
            organization.Id,
            organization.GitHubOrganizationId,
            organization.GitHubOrganizationLogin,
            organization.GitHubCreatedAt);
    }
}
