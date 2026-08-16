using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentForEmployee;

internal sealed class GetAssessmentForEmployeeEndPoint : EndpointWithoutRequest<AssessmentDto>
{
    private readonly IMediator _mediator;

    public GetAssessmentForEmployeeEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/assessment/employee");
        Roles("TeamLeader", "TeamLead", "Employee", "OrganizationOwner", "Admin", "SuperAdmin");
        Options(x => x.WithTags("Assessment"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var assessment = await _mediator.Send(new GetAssessmentForEmployeeQuery(), ct);
        await Send.OkAsync(assessment);
    }
}
