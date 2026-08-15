using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeUserById;

public sealed record GetEmployeeByUserIdCommand() : IRequest<EmployeeDetailsDto>;

public sealed record EmployeeSkillSummaryDto(
    Guid Id,
    Guid SkillId,
    string SkillName,
    int ConfidenceScore);