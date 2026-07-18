// GetEmployeeByIdEndPoint.cs
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Common;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeById;

public sealed class GetEmployeeByIdEndPoint : Endpoint<GetEmployeeByIdRequest, EmployeeDetailsDto>
{
    private readonly IMediator _mediator;

    public GetEmployeeByIdEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/talent-management/employees/{id}");
        Roles(TalentManagementRoles.Privileged.Append("Employee").ToArray());
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(GetEmployeeByIdRequest req, CancellationToken ct)
    {
        var isPrivileged = TalentManagementRoles.Privileged.Any(r => HttpContext.User.IsInRole(r));
        var query = new GetEmployeeByIdCommand(req.Id, isPrivileged);
        var result = await _mediator.Send(query, ct);

        await Send.OkAsync(result, cancellation: ct);
    }
}