// ISkillCatalog.cs
using WorkFit.Skills.Contracts.Dtos;

namespace WorkFit.Skills.Contracts;

public interface ISkillCatalog
{
    Task<SkillResolutionResult> ResolveOrCreateSkillAsync(string rawSkillName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SkillDto>> SearchAsync(string query, int take = 10, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SkillCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
}