using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentsByTeamLeadId;

internal sealed class GetAssessmentsByTeamLeadIdEndPoint : EndpointWithoutRequest<List<AssessmentDto>>
{
    private readonly IMediator _mediator;

    public GetAssessmentsByTeamLeadIdEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }


    public override void Configure()
    {
        Get("api/assessment/teamlead/{TeamLeadId:guid}");
        Roles("TeamLead");
        Options(x => x.WithTags("Assessment"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var teamLeadId = Route<Guid>("TeamLeadId");
        var query = new GetAssessmentsByTeamLeadIdQuery(teamLeadId);
        var assessments = await _mediator.Send(query, ct);
        await Send.OkAsync(assessments);
    }
}
