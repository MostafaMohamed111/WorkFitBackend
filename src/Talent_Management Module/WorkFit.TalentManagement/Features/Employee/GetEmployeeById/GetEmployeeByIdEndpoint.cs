using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeById;

public sealed class GetEmployeeByIdEndPoint : EndpointWithoutRequest<EmployeeDetailsDto>
{
    private readonly IMediator _mediator;

    public GetEmployeeByIdEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/talent-management/employees/{id:guid}");
        Roles("TeamLeader", "OrganizationOwner");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
       var empId = Route<Guid>("id");
        var query = new GetEmployeeByIdCommand(empId);
        var result = await _mediator.Send(query, ct);
        await Send.OkAsync(result, cancellation: ct);
     
       
    }
}