using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.ApproveCandidate;

public sealed class ApproveCandidateEndPoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUser;

    public ApproveCandidateEndPoint(IMediator mediator, ICurrentUserContext currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public override void Configure()
    {
        Patch("/api/recommendations/{id}/candidates/{employeeId}/approve");
        Options(x => x.WithTags("Recommendations"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var recommendationId = Route<Guid>("id");
        var employeeId = Route<Guid>("employeeId");
        var reviewedBy = _currentUser.GetUserId(ct);

        var command = new ApproveCandidateCommand(recommendationId, employeeId, reviewedBy);
        
        await _mediator.Send(command, ct);

        await Send.NoContentAsync(ct);
    }
}
