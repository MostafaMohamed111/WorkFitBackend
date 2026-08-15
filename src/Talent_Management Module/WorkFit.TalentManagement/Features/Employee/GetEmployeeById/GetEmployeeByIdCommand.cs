using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeById;

public sealed record GetEmployeeByUserIdCommand(Guid EmployeeId) : IRequest<EmployeeDetailsDto>;


