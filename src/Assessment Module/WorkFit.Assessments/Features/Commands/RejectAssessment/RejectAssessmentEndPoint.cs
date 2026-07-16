using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Commands.RejectAssessment;

internal sealed class RejectAssessmentEndPoint : Endpoint<RejectAssessmentRequest, Guid>
{
    private readonly IMediator _mediator;

    public RejectAssessmentEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/api/assessments/{id:guid}/reject");
        Roles("TeamLeader", "Employee");
        Options(x => x.WithTags("Assessment"));
    }

    public override async Task HandleAsync(RejectAssessmentRequest request, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new RejectAssessmentCommand(id, request.Note);
        await _mediator.Send(command, ct);
        await Send.OkAsync(cancellation: ct);
    }
}
