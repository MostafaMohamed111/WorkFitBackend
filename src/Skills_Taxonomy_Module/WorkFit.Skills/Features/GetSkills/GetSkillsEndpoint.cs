using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WorkFit.Skills.Infrastructure.Data;

namespace WorkFit.Skills.Features.GetSkills;

public sealed record SkillResponse(Guid Id, string Name);

public sealed class GetSkillsEndpoint : EndpointWithoutRequest<List<SkillResponse>>
{
    private readonly WorkFitSkillsDbContext _context;

    public GetSkillsEndpoint(WorkFitSkillsDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Get("/api/skills");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var skills = await _context.Skills
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SkillResponse(s.Id, s.Name))
            .ToListAsync(ct);

        await Send.OkAsync(skills, ct);
    }
}
