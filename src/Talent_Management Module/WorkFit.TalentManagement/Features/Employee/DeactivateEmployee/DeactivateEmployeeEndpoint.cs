using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.DeactivateEmployee;

public sealed class DeactivateEmployeeEndpoint : Endpoint<DeactivateEmployeeRequest>
{
    private readonly IMediator _mediator;

    public DeactivateEmployeeEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/employees/{Id}");
        Roles("Admin", "HR");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(DeactivateEmployeeRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeactivateEmployeeCommand(req.Id), ct);
        //await SendNoContentAsync(ct);
        await Send.NoContentAsync(ct);
    }
}