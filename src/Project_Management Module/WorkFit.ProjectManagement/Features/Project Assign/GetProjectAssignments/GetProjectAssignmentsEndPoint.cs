using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.GetProjectAssignments;

public sealed class GetProjectAssignmentsEndPoint : Endpoint<GetProjectAssignmentsRequest, List<ProjectAssignmentDto>>
{
    private readonly IMediator _mediator;

    public GetProjectAssignmentsEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/projects/{id}/assignments");
        AllowAnonymous();
        Options(x => x.WithTags("Project Management"));
    }

    public override async Task HandleAsync(GetProjectAssignmentsRequest req, CancellationToken ct)
    {
        var projectId = Route<Guid>("id");

        var query = new GetProjectAssignmentsQuery(projectId, req.IsActive);
        var result = await _mediator.Send(query, ct);
        await Send.OkAsync(result, ct);
    }
}
