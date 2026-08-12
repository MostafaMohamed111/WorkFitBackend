using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.ActivateEmployee;

public sealed record ActivateEmployeeCommand(Guid EmployeeId) : IRequest;
