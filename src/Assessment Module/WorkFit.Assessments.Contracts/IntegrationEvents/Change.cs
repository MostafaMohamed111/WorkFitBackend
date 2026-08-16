namespace WorkFit.Assessments.Contracts.IntegrationEvents;

public sealed record class Change(Guid SkillId, string SkillName, int NewScore, string evidence);
