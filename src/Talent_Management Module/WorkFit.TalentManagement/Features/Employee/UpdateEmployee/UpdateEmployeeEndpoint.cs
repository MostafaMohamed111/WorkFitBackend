using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.UpdateEmployee;

public sealed class UpdateEmployeeEndpoint : Endpoint<UpdateEmployeeRequest>
{
    private readonly IMediator _mediator;

    public UpdateEmployeeEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/employees/{Id}");
        Roles("Admin", "HR");
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(UpdateEmployeeRequest req, CancellationToken ct)
    {
        var command = new UpdateEmployeeCommand(req.Id, req.FirstName, req.LastName, req.JobTitle);

        await _mediator.Send(command, ct);

        //await SendNoContentAsync(ct);
        await Send.NoContentAsync(ct);
    }
}