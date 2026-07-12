using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.GetSkillConfidenceChange;

public sealed class GetSkillConfidenceChangeEndpoint
    : Endpoint<GetSkillConfidenceChangeRequest, SkillConfidenceChangeDto>
{
    private readonly IMediator _mediator;

    public GetSkillConfidenceChangeEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/talent-management/skill-confidence-changes/{id}");
        Options(x => x.WithTags("Talent Management"));
      
    }

    public override async Task HandleAsync(GetSkillConfidenceChangeRequest req, CancellationToken ct)
    {
        var query = new GetSkillConfidenceChangeCommand(req.Id);
        var result = await _mediator.Send(query, ct);

        await Send.OkAsync(result, ct);
    }
}