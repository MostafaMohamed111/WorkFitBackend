namespace WorkFit.TalentManagement.Features.Employee.OnboardEmployee;

public sealed class OnboardEmployeeRequest
{
    public Guid OrganizationId { get; set; }
    public Guid DepartmentId { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string JobTitle { get; set; } = default!;
    public DateTime HireDate { get; set; }
}