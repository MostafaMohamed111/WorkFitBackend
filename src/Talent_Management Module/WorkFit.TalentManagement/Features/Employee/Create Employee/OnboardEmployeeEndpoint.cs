using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.OnboardEmployee;

public sealed class OnboardEmployeeEndpoint
    : Endpoint<OnboardEmployeeRequest, OnboardEmployeeResponse>
{
    private readonly IMediator _mediator;

    public OnboardEmployeeEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/employees");
        Roles("Admin", "HR");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(OnboardEmployeeRequest req, CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirst("sub")!.Value);

        var command = new OnboardEmployeeCommand(
            req.OrganizationId, userId,
            req.Email, 
            req.JobTitle, req.HireDate, req.Name);

        var result = await _mediator.Send(command, ct);

        await Send.OkAsync(result, ct);
    }
}