
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.ProjectManagement.Features.Project.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.Queries.GetProjectsForTeamLead;

internal sealed class GetProjectsForTeamLeadEndPoint : EndpointWithoutRequest<IReadOnlyList<ProjectListItemDto>>
{
    private readonly IMediator _mediator;

    public GetProjectsForTeamLeadEndPoint(
            IMediator mediator
        )
    {
        _mediator = mediator;
    }
    public override void Configure()
    {
        Get("/api/projects/teamLead");
        Roles("TeamLeader");
        Options(x => x.WithTags("Project Management"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new GetProjectsForTeamLeadQuery();
        var projects = await _mediator.Send(query, ct);
        await Send.OkAsync(projects, ct);
    }
}
