

namespace WorkFit.TalentManagement.Contracts.WriteServices.CreateOrUpdateSkill;

public sealed record SkillDetails(
        Guid skillId,
        string skillName,
        int skillScore,
        string details,
        string source
        
    );
