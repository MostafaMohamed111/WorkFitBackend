using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.AssignTeamLead;

public sealed class AssignTeamLeadEndpoint : Endpoint<AssignTeamLeadRequest>
{
    private readonly IMediator _mediator;

    public AssignTeamLeadEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/identity/team-leads");
        Roles("OrganizationOwner");
        Options(options => options.WithTags("Identity"));
    }

    public override async Task HandleAsync(AssignTeamLeadRequest request, CancellationToken ct)
    {
        await _mediator.Send(new AssignTeamLeadCommand(request.UserId), ct);
        await Send.NoContentAsync(ct);
    }
}
