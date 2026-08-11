using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.ConnectGitHubIdentity;

public sealed class ConnectGitHubIdentityEndpoint : Endpoint<ConnectGitHubIdentityRequest>
{
    private readonly IMediator _mediator;

    public ConnectGitHubIdentityEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/employees/me/github");
        Roles("Employee", "Admin", "HR", "OrganizationOwner", "SuperAdmin");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(ConnectGitHubIdentityRequest req, CancellationToken ct)
    {
        await _mediator.Send(
            new ConnectGitHubIdentityCommand(req.GitHubAccountId, req.GitHubDisplayName),
            ct);

        await Send.NoContentAsync(ct);
    }
}
