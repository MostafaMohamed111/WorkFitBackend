using WorkFit.TalentManagement.Features.Employee.GetEmployees;

namespace WorkFit.TalentManagement.Compatibility;

public interface IGetEmployeesCompatService
{
    Task<List<EmployeeListItemDto>> GetEmployeesAsync(Guid userId, CancellationToken ct = default);
}
