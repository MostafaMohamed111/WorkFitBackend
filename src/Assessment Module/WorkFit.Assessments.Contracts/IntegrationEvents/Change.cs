namespace WorkFit.Assessments.Contracts.IntegrationEvents;

public sealed record class Change(Guid SkillId, int NewScore, string evidence);
