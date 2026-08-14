using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.ActivateEmployee;

public sealed class ActivateEmployeeEndpoint : Endpoint<ActivateEmployeeRequest>
{
    private readonly IMediator _mediator;

    public ActivateEmployeeEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Patch("/api/employees/{Id}/activate");
        Roles("Admin", "HR");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(ActivateEmployeeRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ActivateEmployeeCommand(request.Id), ct);
        await Send.NoContentAsync(ct);
    }
}
