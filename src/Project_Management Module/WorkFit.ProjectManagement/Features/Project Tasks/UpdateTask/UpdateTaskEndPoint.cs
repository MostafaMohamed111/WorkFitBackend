using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.UpdateTask;
public sealed class UpdateTaskEndPoint : Endpoint<UpdateTaskRequest, Guid>
{
    private readonly IMediator _mediator;
    public UpdateTaskEndPoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/tasks/{id}");
        Options(x => x.WithTags("Project Management"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(UpdateTaskRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("id");
        var command = new UpdateTaskCommand(taskId, req.Title, req.Description, req.Status,
            req.Priority, req.DueDate, req.StoryPoints);

        var result = await _mediator.Send(command, ct);
        await Send.OkAsync(result, ct);
    }
}