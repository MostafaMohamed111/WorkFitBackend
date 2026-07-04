namespace WorkFit.TalentManagement.Features.Employee.UpdateEmployee;

public sealed class UpdateEmployeeRequest
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string JobTitle { get; set; } = default!;
}