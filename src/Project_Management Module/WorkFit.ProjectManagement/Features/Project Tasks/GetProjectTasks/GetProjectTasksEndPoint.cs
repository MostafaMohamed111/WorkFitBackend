using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.GetProjectTasks;

public sealed class GetProjectTasksEndPoint : Endpoint<GetProjectTasksRequest, List<TaskListItemDto>>
{
    private readonly IMediator _mediator;
    public GetProjectTasksEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/projects/{id}/tasks");
        AllowAnonymous();
        Options(x => x.WithTags("Project Management"));
    }

    public override async Task HandleAsync(GetProjectTasksRequest req, CancellationToken ct)
    {
        var projectId = Route<Guid>("id");
        var query = new GetProjectTasksQuery(projectId, req.Status, req.AssigneeId, req.TaskType, req.Priority);
        var result = await _mediator.Send(query, ct);
        await Send.OkAsync(result, ct);
    }
}