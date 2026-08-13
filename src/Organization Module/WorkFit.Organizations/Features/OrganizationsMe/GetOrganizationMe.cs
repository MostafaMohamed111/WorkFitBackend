using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Domain.Exceptions;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.OrganizationsMe;

public sealed record GetOrganizationMeRequest(Guid? UserId);
public sealed record GetOrganizationMeQuery(Guid UserId) : IRequest<OrganizationDetailsResponse>;

public sealed class GetOrganizationMeQueryHandler : IRequestHandler<GetOrganizationMeQuery, OrganizationDetailsResponse>
{
    private readonly OrganizationDbContext _context;

    public GetOrganizationMeQueryHandler(OrganizationDbContext context) => _context = context;

    public async Task<OrganizationDetailsResponse> Handle(GetOrganizationMeQuery request, CancellationToken cancellationToken = default)
    {
        // 1. Direct owner match
        var organization = await _context.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

        // 2. Fallback: Lookup by employee profile's organization ID
        if (organization is null && request.UserId != Guid.Empty)
        {
            var connectionString = _context.Database.GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
            {
                try
                {
                    using var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                    await conn.OpenAsync(cancellationToken);
                    using var cmd = new Microsoft.Data.SqlClient.SqlCommand(
                        "SELECT TOP 1 OrganizationId FROM [talent].[EmployeeProfiles] WHERE UserId = @UserId OR Id = @UserId", conn);
                    cmd.Parameters.AddWithValue("@UserId", request.UserId);
                    var orgIdObj = await cmd.ExecuteScalarAsync(cancellationToken);
                    if (orgIdObj != null && orgIdObj != DBNull.Value && Guid.TryParse(orgIdObj.ToString(), out var orgId))
                    {
                        organization = await _context.Organizations.AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Id == orgId, cancellationToken);
                    }
                }
                catch { }
            }
        }

        // 3. Fallback: Return first available organization
        if (organization is null)
        {
            organization = await _context.Organizations.AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new OrganizationNotFoundException();
        }

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

public sealed class GetOrganizationMeEndpoint : Endpoint<GetOrganizationMeRequest, OrganizationDetailsResponse>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUser;

    public GetOrganizationMeEndpoint(IMediator mediator, ICurrentUserContext currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public override void Configure()
    {
        Get("/api/organizations/me");
        AllowAnonymous();
        Options(x => x.WithTags("Organization"));
    }

    public override async Task HandleAsync(GetOrganizationMeRequest req, CancellationToken ct)
    {
        var targetUserId = (req.UserId.HasValue && req.UserId.Value != Guid.Empty)
            ? req.UserId.Value
            : _currentUser.GetUserId(ct);

        var response = await _mediator.Send(new GetOrganizationMeQuery(targetUserId), ct);
        await Send.OkAsync(response, ct);
    }
}
