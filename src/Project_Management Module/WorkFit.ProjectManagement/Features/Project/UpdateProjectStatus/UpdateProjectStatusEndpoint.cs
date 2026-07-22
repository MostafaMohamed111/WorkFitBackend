using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.UpdateProjectStatus;

public sealed class UpdateProjectStatusEndpoint : Endpoint<UpdateProjectStatusRequest, Guid>
{
    private readonly IMediator _mediator;

    public UpdateProjectStatusEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Put("/api/projects/{id}/status");
        Options(x => x.WithTags("Project Management"));
        Roles("TeamLeader");
        Description(b => b
            .Produces<Guid>(200)
            .ProducesProblem(400)
            .Produces(404));

    }

    public override async Task HandleAsync(UpdateProjectStatusRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateProjectStatusCommand(req.Id, req.Status), ct);

 

        await Send.OkAsync(result, ct);
    }
}
