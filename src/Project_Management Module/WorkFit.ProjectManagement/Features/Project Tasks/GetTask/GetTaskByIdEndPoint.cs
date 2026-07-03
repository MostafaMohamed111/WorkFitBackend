using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.ProjectManagement.Features.Project_Tasks.UpdateTask;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.GetTask;
public sealed class GetTaskByIdEndPoint : EndpointWithoutRequest<TaskDetailDto>
{
    private readonly IMediator _mediator;
    public GetTaskByIdEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    } 

    public override void Configure()
    {
        Get("/api/tasks/{id}");
        AllowAnonymous();
        Options(x => x.WithTags("Project Management"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var taskId = Route<Guid>("id");
        var result = await _mediator.Send(new GetTaskByIdQuery(taskId), ct);
        await Send.OkAsync(result, ct);
    }
}