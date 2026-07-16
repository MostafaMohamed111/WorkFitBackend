

using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.ApproveAssessment;

internal sealed class ApproveAssessmentEndPoint : Endpoint<ApproveAssessmentRequest,Guid>
{
    private readonly IMediator _mediator;

    public ApproveAssessmentEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    public override void Configure()
    {
        Put("/api/assessments/{id:guid}/approve");
        Roles("TeamLeader", "Employee");
        Options(x => x.WithTags("Assessment"));
    }

    public override async Task HandleAsync(ApproveAssessmentRequest request, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new ApproveAssessmentCommand(id, request.Note);
        await _mediator.Send(command, ct);
        await Send.OkAsync(cancellation: ct);
    }
}
