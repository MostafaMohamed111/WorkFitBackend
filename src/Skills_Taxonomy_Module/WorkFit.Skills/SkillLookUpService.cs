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
        throw new NotImplementedException();
    }

    Task<SkillLookUpDto?> ISkillLookUpService.GetSkillByIdAsync(Guid skillId)
    {
        throw new NotImplementedException();
    }

    Task<IReadOnlyCollection<SkillLookUpDto>> ISkillLookUpService.GetSkillsByIdsAsync(IEnumerable<Guid> skillIds)
    {
        throw new NotImplementedException();
    }
}