using FastEndpoints;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.CreateAssignment;

public sealed class CreateAssignmentEndPoint : Endpoint<CreateAssignmentRequest, CreateAssignmentResponse>
{
    private readonly IMediator _mediator;

    public CreateAssignmentEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/projects/{id}/assignments");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateAssignmentRequest req, CancellationToken ct)
    {
        var projectId = Route<Guid>("id");

        var command = new CreateAssignmentCommand(
            projectId,
            req.EmployeeId,
            req.RoleOnProject,
            req.AllocationPercentage,
            req.StartDate,
            req.EndDate);

        var result = await _mediator.Send(command, ct);
        await Send.OkAsync(result, ct);
    }
}
