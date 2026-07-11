using FastEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Contracts.Dtos;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeSkillDetails;

public sealed class GetEmployeeSkillDetailsEndPoint : Endpoint<GetEmployeeSkillDetailsRequest, EmployeeSkillDetailsDto>
{
    private readonly IMediator _mediator;

    public GetEmployeeSkillDetailsEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/talent-management/employee-skills/{id}");
        Roles("TeamLeader");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(GetEmployeeSkillDetailsRequest req, CancellationToken ct)
    {
        var query = new GetEmployeeSkillDetailsCommand(req.Id);
        var result = await _mediator.Send(query, ct);

        await Send.OkAsync(result, cancellation: ct);
    }
}