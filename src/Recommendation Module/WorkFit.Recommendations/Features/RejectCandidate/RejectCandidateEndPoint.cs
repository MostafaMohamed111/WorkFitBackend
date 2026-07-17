using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.RejectCandidate;

public sealed record RejectCandidateRequest(Guid ActionedBy);

public sealed class RejectCandidateEndPoint : Endpoint<RejectCandidateRequest>
{
    private readonly IMediator _mediator;

    public RejectCandidateEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Patch("/api/recommendations/{id}/candidates/{employeeId}/reject");
        AllowAnonymous();
        Options(x => x.WithTags("Recommendations"));
    }

    public override async Task HandleAsync(RejectCandidateRequest req, CancellationToken ct)
    {
        var recommendationId = Route<Guid>("id");
        var employeeId = Route<Guid>("employeeId");

        var command = new RejectCandidateCommand(recommendationId, employeeId, req.ActionedBy);
        
        await _mediator.Send(command, ct);

        await Send.NoContentAsync(ct);
    }
}
