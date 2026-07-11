namespace WorkFit.TalentManagement.Contracts.Dtos;

public sealed record EmployeeSkillDetailsDto(
	Guid EmployeeSkillId,
	Guid EmployeeProfileId,
	Guid SkillId,
	string SkillName,
	int ConfidenceScore,
	IReadOnlyList<ConfidenceChangeSummaryDto> ConfidenceChanges
	);


