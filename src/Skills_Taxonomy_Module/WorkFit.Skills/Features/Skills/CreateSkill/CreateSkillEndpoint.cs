using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.ICurrentUser;

namespace WorkFit.Skills.Features.Skills.CreateSkill;

public sealed class CreateSkillEndpoint : Endpoint<CreateSkillRequest, SkillDto>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUser;

    public CreateSkillEndpoint(IMediator mediator, ICurrentUserContext currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public override void Configure()
    {
        Post("/api/skills");
        AllowAnonymous(); 
        Summary(s =>
        {
            s.Summary = "Create a new skill";
            s.Description = "Creates a new skill in the taxonomy";
        });
    }

    public override async Task HandleAsync(CreateSkillRequest req, CancellationToken ct)
    {
        var organizationId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var command = new CreateSkillCommand(
            req.Name,
            req.Description,
            req.Origin,
            organizationId,
            req.CategoryId,
            req.GroupId,
            req.ParentSkillId,
            req.EstimatedTimeToLearn
        );

        var result = await _mediator.Send(command, ct);

        await SendAsync(result, StatusCodes.Status201Created, ct);
    }
}