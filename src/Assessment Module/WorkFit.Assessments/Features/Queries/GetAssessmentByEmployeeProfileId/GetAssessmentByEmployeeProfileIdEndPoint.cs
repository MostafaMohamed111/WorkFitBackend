using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Assessments.Features.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.Queries.GetAssessmentByEmployeeProfileId;

internal sealed class GetAssessmentByEmployeeProfileIdEndPoint : EndpointWithoutRequest<AssessmentDto>
{
    private readonly IMediator _mediator;

    public GetAssessmentByEmployeeProfileIdEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/assessment/employee-profile/{EmployeeProfileId:guid}");
        Roles("TeamLead", "Employee");
        Options(x => x.WithTags("Assessment"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var employeeProfileId = Route<Guid>("EmployeeProfileId");
        var query = new GetAssessmentByEmployeeProfileIdQuery(employeeProfileId);
        var assessment = await _mediator.Send(query, ct);
        await Send.OkAsync(assessment);
    }
}
