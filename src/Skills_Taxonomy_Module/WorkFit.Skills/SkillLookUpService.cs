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

    public async Task<SkillLookUpDto?> GetSkillByIdAsync(Guid skillId)
    {
        return await _db.Skills
            .Where(s => s.Id == skillId && !s.IsDeleted)
            .Select(s => new SkillLookUpDto(s.Id, s.Name, s.CategoryId, s.GroupId))
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyCollection<SkillLookUpDto>> GetSkillsByIdsAsync(IEnumerable<Guid> skillIds)
    {
        var ids = skillIds.Distinct().ToList();
        if (!ids.Any()) return Array.Empty<SkillLookUpDto>();

        return await _db.Skills
            .Where(s => ids.Contains(s.Id) && !s.IsDeleted)
            .Select(s => new SkillLookUpDto(s.Id, s.Name, s.CategoryId, s.GroupId))
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid skillId)
    {
        return await _db.Skills.AnyAsync(s => s.Id == skillId && !s.IsDeleted);
    }
}