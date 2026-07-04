using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.Employee.GetEmployees;

public sealed record GetEmployeesQuery(Guid OrgId) : IRequest<List<EmployeeListItemDto>>;