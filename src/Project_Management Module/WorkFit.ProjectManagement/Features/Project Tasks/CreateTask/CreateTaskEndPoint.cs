using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.CreateTask;

public sealed class CreateTaskEndPoint : Endpoint<CreateTaskRequest, Guid>
{
    private readonly IMediator _mediator;
    public CreateTaskEndPoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/projects/{id}/tasks");
        Options(x => x.WithTags("Project Management"));
        Roles("TeamLeader");
    }

    public override async Task HandleAsync(CreateTaskRequest req, CancellationToken ct)
    {
        var projectId = Route<Guid>("id");
        var command = new CreateTaskCommand(projectId, req.Title, req.Description, req.TaskType,
            req.Priority, req.AssigneeId, req.StoryPoints, req.DueDate, req.AllocationPercentage    );

        var taskId = await _mediator.Send(command, ct);
        await Send.OkAsync(taskId);
    }
}