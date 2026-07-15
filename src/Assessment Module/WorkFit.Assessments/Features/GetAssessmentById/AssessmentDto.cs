namespace WorkFit.Assessments.Features.GetAssessmentById;

internal sealed record AssessmentDto(
        Guid AssessmentId,
        Guid EmployeeId,
        Guid? TaskId,
        List<SkillChangeDto> SkillChanges
    );