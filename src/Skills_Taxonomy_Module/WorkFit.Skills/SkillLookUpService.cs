using Microsoft.EntityFrameworkCore;
using WorkFit.Skills.Contracts.SkillLookUp;
using WorkFit.Skills.Infrastructure.Data;

namespace WorkFit.Skills;

internal sealed class SkillLookUpService : ISkillLookUpService
{
    private readonly WorkFitSkillsDbContext _db;

    public SkillLookUpService(WorkFitSkillsDbContext db)
    {
        _db = db;
    }

    Task<bool> ISkillLookUpService.ExistsAsync(Guid skillId)
    {
        return _db.Skills.AsNoTracking().AnyAsync(skill => skill.Id == skillId && !skill.IsDeleted);
    }

    Task<SkillLookUpDto?> ISkillLookUpService.GetSkillByIdAsync(Guid skillId)
    {
        return _db.Skills.AsNoTracking()
            .Where(skill => skill.Id == skillId && !skill.IsDeleted)
            .Select(skill => new SkillLookUpDto(skill.Id, skill.Name, skill.CategoryId, null))
            .FirstOrDefaultAsync();
    }

    async Task<IReadOnlyCollection<SkillLookUpDto>> ISkillLookUpService.GetSkillsByIdsAsync(IEnumerable<Guid> skillIds)
    {
        ArgumentNullException.ThrowIfNull(skillIds);
        var ids = skillIds.Where(id => id != Guid.Empty).Distinct().ToArray();
        return await _db.Skills.AsNoTracking()
            .Where(skill => ids.Contains(skill.Id) && !skill.IsDeleted)
            .Select(skill => new SkillLookUpDto(skill.Id, skill.Name, skill.CategoryId, null))
            .ToListAsync();
    }
}
