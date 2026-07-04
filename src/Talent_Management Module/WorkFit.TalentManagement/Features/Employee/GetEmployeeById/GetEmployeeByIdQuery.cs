using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeById;

public sealed record GetEmployeeByIdQuery(Guid EmployeeId) : IRequest<EmployeeDetailDto>;