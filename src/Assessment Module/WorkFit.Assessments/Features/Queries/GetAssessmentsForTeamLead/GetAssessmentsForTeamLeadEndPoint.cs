using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentsForTeamLead;

internal sealed class GetAssessmentsForTeamLeadEndPoint : EndpointWithoutRequest<List<AssessmentDto>>
{
    private readonly IMediator _mediator;

    public GetAssessmentsForTeamLeadEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/assessment/teamlead");
        Roles("TeamLeader", "TeamLead", "Employee", "OrganizationOwner", "Admin", "SuperAdmin");
        Options(x => x.WithTags("Assessment"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var assessments = await _mediator.Send(new GetAssessmentsForTeamLeadQuery(), ct);
        await Send.OkAsync(assessments);
    }
}
