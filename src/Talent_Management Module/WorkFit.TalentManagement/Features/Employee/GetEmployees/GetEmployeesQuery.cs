using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployees;

public sealed record GetEmployeesQuery() : IRequest<List<EmployeeListItemDto>>;