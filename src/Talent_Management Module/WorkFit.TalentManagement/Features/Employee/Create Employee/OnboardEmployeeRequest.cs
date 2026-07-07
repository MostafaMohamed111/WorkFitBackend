namespace WorkFit.TalentManagement.Features.Employee.OnboardEmployee;

public sealed class OnboardEmployeeRequest
{
    public Guid OrganizationId { get; set; }
    public string Email { get; set; } = default!;
    public string JobTitle { get; set; } = default!;
    public DateOnly? HireDate { get; set; }
    public string Name { get; set; } = default!;
}