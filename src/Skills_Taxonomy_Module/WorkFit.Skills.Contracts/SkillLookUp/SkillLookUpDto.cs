namespace WorkFit.Skills.Contracts.SkillLookUp;

public sealed record SkillLookUpDto(
    Guid SkillId,
    string Name,
    Guid? CategoryId,
    Guid? GroupId
);