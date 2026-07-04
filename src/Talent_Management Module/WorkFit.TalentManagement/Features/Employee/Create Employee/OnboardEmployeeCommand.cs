using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.OnboardEmployee;

public sealed record OnboardEmployeeCommand(
    Guid OrganizationId,
    Guid DepartmentId,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string JobTitle,
    DateTime HireDate
) : IRequest<OnboardEmployeeResponse>;