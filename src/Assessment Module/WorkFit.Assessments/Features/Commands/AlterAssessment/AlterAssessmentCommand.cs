using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Assessments.Features.AlterAssessment;

internal sealed record AlterAssessmentCommand(Guid AssessmentId, List<AlterAssessmentSkillChange> SkillChanges, string? Note) : IRequest<Guid>;

internal sealed record AlterAssessmentSkillChange(Guid SkillChangeId, int NewScore, string? Note);
