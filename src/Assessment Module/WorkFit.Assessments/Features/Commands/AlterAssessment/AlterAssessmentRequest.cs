namespace WorkFit.Assessments.Features.Commands.AlterAssessment;

internal sealed record AlterAssessmentRequest(List<AlterAssessmentSkillChangeRequest> SkillChanges, string? Note);

internal sealed record AlterAssessmentSkillChangeRequest(Guid SkillChangeId, int NewScore, string? Note);
