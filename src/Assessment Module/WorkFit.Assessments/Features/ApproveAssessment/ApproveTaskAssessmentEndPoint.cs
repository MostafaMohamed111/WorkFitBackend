

using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.ApproveAssessment;

internal sealed class ApproveTaskAssessmentEndPoint : Endpoint<ApproveTaskAssessmentRequest,Guid>
{
    private readonly IMediator _mediator;

    public ApproveTaskAssessmentEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    public override void Configure()
    {
        Put("/api/assessments/{id:guid}/approve");
        Roles("TeamLeader");
        Options(x => x.WithTags("Assessment"));
    }

    public override async Task HandleAsync(ApproveTaskAssessmentRequest request, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new ApproveTaskAssessmentCommand(id, request.Note);
        await _mediator.Send(command, ct);
        await Send.OkAsync(cancellation: ct);
    }
}
