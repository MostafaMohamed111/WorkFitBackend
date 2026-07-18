using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeById;

public sealed record GetEmployeeByIdCommand(Guid EmployeeId, bool IsPrivilegedCaller) : IRequest<EmployeeDetailsDto>;

public sealed record EmployeeDetailsDto(
    Guid EmployeeId,
    Guid OrganizationId,
    Guid UserId,
    string Name,
    string Email,
    string JobTitle,
    string? Bio,
    string? LinkedInUrl,
    string Status,
    int CurrentAllocationPercentage,
    IReadOnlyList<EmployeeSkillSummaryDto> Skills);

public sealed record EmployeeSkillSummaryDto(
    Guid Id,
    Guid SkillId,
    string SkillName,
    int ConfidenceScore);