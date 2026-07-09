namespace WorkFit.TalentManagement.Contracts.WriteServices.CreateEmployee;

public interface ICreateEmployeeService
{
    // returns the employee id of the newly created employee
    Task<Guid> CreateEmployeeAsync(EmployeeDetails employeeDetails, CancellationToken cancellationToken = default);
}
