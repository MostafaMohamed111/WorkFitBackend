using WorkFit.SharedKernel.MediatorContract;
using WorkFit.Skills.Contracts;
using WorkFit.Skills.Contracts.Dtos;
using WorkFit.Skills.Features.ResolveOrCreateSkill;

namespace WorkFit.Skills.Features;

/// Implements the cross-module contract. Every method just forwards to
/// the same handler an in-module FastEndpoints call would use — one
/// code path regardless of caller.

public sealed class SkillCatalogService : ISkillCatalog
{
    private readonly IMediator _mediator;

    public SkillCatalogService(IMediator mediator) => _mediator = mediator;

    public Task<SkillResolutionResult> ResolveOrCreateSkillAsync(string rawSkillName, CancellationToken cancellationToken = default)
     => _mediator.Send(new ResolveOrCreateSkillCommand(rawSkillName), cancellationToken);

    public Task<IReadOnlyList<SkillDto>> SearchAsync(string query, int take = 10, CancellationToken cancellationToken = default)
        => throw new NotImplementedException("SearchSkills feature not built yet.");

    public Task<IReadOnlyList<SkillCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException("GetSkillCategories feature not built yet.");
}