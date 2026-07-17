using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.GetTaskRecommendations;

public sealed class GetTaskRecommendationsEndPoint
    : EndpointWithoutRequest<List<RecommendationListItemDto>>
{
    private readonly IMediator _mediator;
    public GetTaskRecommendationsEndPoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/tasks/{taskId}/recommendations");
        AllowAnonymous();
        Options(x => x.WithTags("Recommendations"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var taskId = Route<Guid>("taskId");
        var result = await _mediator.Send(new GetTaskRecommendationsQuery(taskId), ct);
        await Send.OkAsync(result, ct);
    }
}
