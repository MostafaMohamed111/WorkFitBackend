namespace WorkFit.Skills.Contracts.SkillLookUp;

public interface ISkillLookUpService
{
    Task<SkillLookUpDto?> GetSkillByIdAsync(Guid skillId);
    Task<IReadOnlyCollection<SkillLookUpDto>> GetSkillsByIdsAsync(IEnumerable<Guid> skillIds);
    Task<bool> ExistsAsync(Guid skillId);
}