using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.WorkFlow.Features.GenerateEmployeeRecommendation;

public sealed class GenerateEmployeeRecommendationEndPoint
    : Endpoint<GenerateEmployeeRecommendationRequest, GenerateEmployeeRecommendationResponse>
{
    private readonly IMediator _mediator;

    public GenerateEmployeeRecommendationEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/workflow/tasks/{taskId}/employee-recommendations");
        Options(x => x.WithTags("WorkFlow", "Recommendations"));
        Roles("TeamLeader");
    }

    public override async Task HandleAsync(
        GenerateEmployeeRecommendationRequest request,
        CancellationToken cancellationToken)
    {
        var taskId = Route<Guid>("taskId");
        var response = await _mediator.Send(
            new GenerateEmployeeRecommendationCommand(taskId, request.Prompt, request.ResultLimit),
            cancellationToken);

        await Send.OkAsync(response, cancellationToken);
    }
}
