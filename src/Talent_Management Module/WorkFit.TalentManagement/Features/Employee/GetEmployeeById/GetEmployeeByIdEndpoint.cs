using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeById;

public sealed class GetEmployeeByIdEndpoint
    : Endpoint<GetEmployeeByIdRequest, EmployeeDetailDto>
{
    private readonly IMediator _mediator;

    public GetEmployeeByIdEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/employees/{Id}");
        Roles("Admin", "HR", "Manager");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(GetEmployeeByIdRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEmployeeByIdQuery(req.Id), ct);
        //await SendAsync(result, statusCode: 200, cancellation: ct);
        await Send.OkAsync(result, ct);
    }
}