using WorkFit.TalentManagement.Contracts.LookUpServices;
using WorkFit.TalentManagement.Contracts.Dtos;

namespace WorkFit.TalentManagement.Contracts.LookUpServices;

public static class EmployeeLookUpServiceExtensions
{
    public static Task<EmployeeDetailsDto?> GetEmployeeByIdAsync(
        this IEmployeeLookUpService service,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return service is WorkFit.TalentManagement.CrossCutting.EmployeeLookUpService lookup
            ? lookup.GetEmployeeByIdAsync(id, cancellationToken)
            : throw new NotSupportedException("The configured employee lookup service does not support lookup operations.");
    }

    public static Task<List<EmployeeDetailsDto>> GetAllEmployeesAsync(
        this IEmployeeLookUpService service,
        CancellationToken cancellationToken = default)
    {
        return service is WorkFit.TalentManagement.CrossCutting.EmployeeLookUpService lookup
            ? lookup.GetAllEmployeesAsync(cancellationToken)
            : throw new NotSupportedException("The configured employee lookup service does not support lookup operations.");
    }
}
