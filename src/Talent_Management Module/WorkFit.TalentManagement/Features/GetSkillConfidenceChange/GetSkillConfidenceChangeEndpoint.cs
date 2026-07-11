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
        AllowAnonymous();
        Options(x => x.WithTags("Talent Management"));
        Summary(s =>
        {
            s.Summary = "Get a skill confidence change by ID";
            s.Description = "Returns detailed information about a specific skill confidence change including evidence";
        });
    }

    public override async Task HandleAsync(GetSkillConfidenceChangeRequest req, CancellationToken ct)
    {
        var query = new GetSkillConfidenceChangeCommand(req.Id);
        var result = await _mediator.Send(query, ct);

        await Send.OkAsync(result, ct);
    }
}