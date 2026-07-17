using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.GetRecommendationById;

public sealed class GetRecommendationByIdEndPoint
    : EndpointWithoutRequest<RecommendationDetailDto>
{
    private readonly IMediator _mediator;
    public GetRecommendationByIdEndPoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/recommendations/{id}");
        AllowAnonymous();
        Options(x => x.WithTags("Recommendations"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        var result = await _mediator.Send(new GetRecommendationByIdQuery(id), ct);
        await Send.OkAsync(result, ct);
    }
}
