
using WorkFit.TalentManagement.Contracts.WriteServices.CreateEmployee;

namespace WorkFit.TalentManagement.CrossCutting;

internal sealed class CreateEmployeeService : ICreateEmployeeService
{
    public Task<Guid> CreateEmployeeAsync(EmployeeDetails employeeDetails, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
