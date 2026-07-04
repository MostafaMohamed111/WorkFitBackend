namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeById;

public sealed record EmployeeDetailDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string JobTitle,
    DateTime HireDate,
    bool IsActive,
    int AvailabilityPercentage,
    bool MobilityReady,
    string? Bio);