

using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentById;

internal class GetAssessmentByIdEndPoint : EndpointWithoutRequest<AssessmentDto>
{
    private readonly IMediator _mediator;

    public GetAssessmentByIdEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }
    public override void Configure()
    {
        Get("/api/assessment/{Id:guid}");
        Roles("TeamLead","Employee");
        Options(x => x.WithTags("Assessment"));
    }
    public override async Task HandleAsync(CancellationToken ct)
    {
        var assessmentId = Route<Guid>("Id");
        var query = new GetAssessmentByIdQuery(assessmentId);

        var assessmentDto =await _mediator.Send(query, ct);
       
        await Send.OkAsync(assessmentDto);

    }
}
