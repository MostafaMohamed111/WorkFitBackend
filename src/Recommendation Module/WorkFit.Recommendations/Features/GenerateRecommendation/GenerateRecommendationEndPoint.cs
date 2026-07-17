using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Recommendations.Features.GetRecommendationById;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Recommendations.Features.GenerateRecommendation;

public sealed class GenerateRecommendationEndPoint
    : Endpoint<GenerateRecommendationRequest, GenerateRecommendationResponse>
{
    private readonly IMediator _mediator;
    public GenerateRecommendationEndPoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/tasks/{taskId}/recommendations");
        Options(x => x.WithTags("Recommendations"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(GenerateRecommendationRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("taskId");

        var command = new GenerateRecommendationCommand(
            taskId,
            req.RequiredSkillIds);

        var result = await _mediator.Send(command, ct);

        await Send.CreatedAtAsync<GetRecommendationByIdEndPoint>(
            new { id = result.Id }, result, cancellation: ct);
    }
}
