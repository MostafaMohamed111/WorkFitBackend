using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.DeactivateEmployee;

public sealed record DeactivateEmployeeCommand(Guid EmployeeId) : IRequest;