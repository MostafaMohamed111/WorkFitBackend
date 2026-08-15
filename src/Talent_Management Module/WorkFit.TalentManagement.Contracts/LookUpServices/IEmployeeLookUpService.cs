using WorkFit.TalentManagement.Contracts.Dtos;

namespace WorkFit.TalentManagement.Contracts.LookUpServices;

public interface IEmployeeLookUpService
{
    Task<EmployeeDetailsDto?> GetEmployeeByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<EmployeeDetailsDto?> GetEmployeeByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<List<EmployeeDetailsDto>> GetAllEmployeesAsync(
        CancellationToken cancellationToken = default);

    Task<List<EmployeeDetailsDto>> GetEmployeesByOrganizationIdAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
