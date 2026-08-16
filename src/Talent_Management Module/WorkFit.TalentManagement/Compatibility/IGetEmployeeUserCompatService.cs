using WorkFit.TalentManagement.Contracts.Dtos;

namespace WorkFit.TalentManagement.Compatibility;

public interface IGetEmployeeUserCompatService
{
    Task<EmployeeDetailsDto?> GetEmployeeUserAsync(Guid userId, CancellationToken ct = default);
}
