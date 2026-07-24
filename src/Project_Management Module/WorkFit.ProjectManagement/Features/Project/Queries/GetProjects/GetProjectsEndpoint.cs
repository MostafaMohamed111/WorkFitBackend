using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.ProjectManagement.Features.Project.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.Queries.GetProjects;

public sealed class GetProjectsEndpoint
    : Endpoint<GetProjectsRequest, IReadOnlyList<ProjectListItemDto>>
{
    private readonly IMediator _mediator;

    public GetProjectsEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/projects");
        AllowAnonymous();
        // Roles(...);
        // Policies(...);
        Options(x => x.WithTags("Project Management"));
    }

    public override async Task HandleAsync(
        GetProjectsRequest req,
        CancellationToken ct)
    {
        var query = new GetProjectsQuery(
            req.Status,
            req.OrganizationId,
            req.Page,
            req.Limit);

        var result = await _mediator.Send(query, ct);

        await Send.OkAsync(result, ct);
    }
}