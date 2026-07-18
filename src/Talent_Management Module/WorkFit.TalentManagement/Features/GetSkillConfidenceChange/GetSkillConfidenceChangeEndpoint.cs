using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Common;

namespace WorkFit.TalentManagement.Features.GetSkillConfidenceChange;

public sealed class GetSkillConfidenceChangeEndpoint
    : Endpoint<GetSkillConfidenceChangeRequest, SkillConfidenceChangeDto>
{
    private readonly IMediator _mediator;

    public GetSkillConfidenceChangeEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Endpoint
    public override void Configure()
    {
        Get("/api/talent-management/skill-confidence-changes/{id}");
        Roles(TalentManagementRoles.Privileged.Append("Employee").ToArray());
        Options(x => x.WithTags("Talent Management"));
    }

    public override async Task HandleAsync(GetSkillConfidenceChangeRequest req, CancellationToken ct)
    {
        var isPrivileged = TalentManagementRoles.Privileged.Any(r => HttpContext.User.IsInRole(r));
        var query = new GetSkillConfidenceChangeCommand(req.Id, isPrivileged);
        var result = await _mediator.Send(query, ct);
        await Send.OkAsync(result, ct);
    }
}