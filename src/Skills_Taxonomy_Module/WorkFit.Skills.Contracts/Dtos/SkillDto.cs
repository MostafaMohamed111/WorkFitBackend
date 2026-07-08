namespace WorkFit.Skills.Contracts.Dtos;

public sealed record SkillDto(
    Guid Id,
    string Name,
    Guid? CategoryId
    );