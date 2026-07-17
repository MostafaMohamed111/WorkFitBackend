using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.ApproveCandidate;

public sealed record ApproveCandidateRequest(Guid ActionedBy);

public sealed class ApproveCandidateEndPoint : Endpoint<ApproveCandidateRequest>
{
    private readonly IMediator _mediator;

    public ApproveCandidateEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Patch("/api/recommendations/{id}/candidates/{employeeId}/approve");
        AllowAnonymous();
        Options(x => x.WithTags("Recommendations"));
    }

    public override async Task HandleAsync(ApproveCandidateRequest req, CancellationToken ct)
    {
        var recommendationId = Route<Guid>("id");
        var employeeId = Route<Guid>("employeeId");

        var command = new ApproveCandidateCommand(recommendationId, employeeId, req.ActionedBy);
        
        await _mediator.Send(command, ct);

        await Send.NoContentAsync(ct);
    }
}
