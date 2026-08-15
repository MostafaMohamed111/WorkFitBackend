

using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Features.Employee.GetEmployeeUserById;


namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeByUserId;

public sealed class GetEmployeeByUserIdEndPoint : EndpointWithoutRequest<EmployeeDetailsDto>
{
    private readonly IMediator _mediator;

    public GetEmployeeByUserIdEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/talent-management/employees/");
        Roles("TeamLeader", "OrganizationOwner", "SuperAdmin", "Employee");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new GetEmployeeByUserIdCommand();
        var result = await _mediator.Send(query, ct);

        await Send.OkAsync(result, cancellation: ct);
    }
}