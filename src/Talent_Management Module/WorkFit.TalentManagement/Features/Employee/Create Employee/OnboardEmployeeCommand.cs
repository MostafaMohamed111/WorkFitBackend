using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.OnboardEmployee;

public sealed record OnboardEmployeeCommand(
    Guid OrganizationId,
    Guid UserId,
    string Email,
    string JobTitle,
    DateOnly? HireDate,
    string Name
) : IRequest<OnboardEmployeeResponse>;