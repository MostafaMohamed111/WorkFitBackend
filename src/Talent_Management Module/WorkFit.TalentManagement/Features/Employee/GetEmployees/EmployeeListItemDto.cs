namespace WorkFit.TalentManagement.Features.Employee.GetEmployees;

public sealed record EmployeeListItemDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string JobTitle,
    bool IsActive,
    int AvailabilityPercentage);