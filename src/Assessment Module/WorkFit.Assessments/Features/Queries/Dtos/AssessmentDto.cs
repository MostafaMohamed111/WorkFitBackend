namespace WorkFit.Assessments.Features.Queries.Dtos;

internal sealed record AssessmentDto(
        Guid AssessmentId,
        Guid EmployeeId,
        Guid? TaskId,
        List<SkillChangeDto> SkillChanges
    );