using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployees;

public sealed class GetEmployeesEndpoint
    : Endpoint<GetEmployeesRequest, List<EmployeeListItemDto>>
{
    private readonly IMediator _mediator;

    public GetEmployeesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/employees");
        Roles("Admin", "HR", "Manager");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(GetEmployeesRequest req, CancellationToken ct)
    {
        var orgId = Guid.Parse(User.FindFirst("OrgId")!.Value);

        var result = await _mediator.Send(new GetEmployeesQuery(orgId), ct);

        //await SendAsync(result, statusCode: 200, cancellation: ct);
        await Send.OkAsync(result, ct);
    }
}