using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.Skills.Contracts.Dtos;

namespace WorkFit.Skills.Features.ResolveOrCreateSkill;

public sealed class ResolveOrCreateSkillEndPoint : Endpoint<ResolveOrCreateSkillRequest, SkillResolutionResult>
{
    private readonly IMediator _mediator;

    public ResolveOrCreateSkillEndPoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/skills/resolve-or-create");
        Options(x => x.WithTags("Skills"));
        AllowAnonymous();
    }

    public override async Task HandleAsync(ResolveOrCreateSkillRequest req, CancellationToken ct)
    {
        var command = new ResolveOrCreateSkillCommand(req.RawSkillName);
        var result = await _mediator.Send(command, ct);

        await Send.OkAsync(result, cancellation: ct);
    }
}