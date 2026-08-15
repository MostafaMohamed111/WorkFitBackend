using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeById;

public sealed record GetEmployeeByIdCommand(Guid EmployeeId) : IRequest<EmployeeDetailsDto>;


