namespace WorkFit.Skills.Contracts.Dtos;

public sealed record SkillResolutionResult(Guid SkillId, string Name, SkillResolutionMethod Method);