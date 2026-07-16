using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.AlterAssessment;

internal sealed class AlterAssessmentEndPoint : Endpoint<AlterAssessmentRequest, Guid>
{
    private readonly IMediator _mediator;

    public AlterAssessmentEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/api/assessments/{id:guid}/alter");
        Roles("TeamLeader", "Employee");
        Options(x => x.WithTags("Assessment"));
    }

    public override async Task HandleAsync(AlterAssessmentRequest request, CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var command = new AlterAssessmentCommand(
            id,
            request.SkillChanges.Select(sc => new AlterAssessmentSkillChange(sc.SkillChangeId, sc.NewScore, sc.Note)).ToList(),
            request.Note
        );

        await _mediator.Send(command, ct);
        await Send.OkAsync(cancellation: ct);
    }
}
