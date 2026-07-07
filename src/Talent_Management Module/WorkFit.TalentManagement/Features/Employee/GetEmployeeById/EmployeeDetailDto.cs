namespace WorkFit.TalentManagement.Features.Employee.GetEmployeeById;

public sealed record EmployeeDetailDto(
    Guid Id,
    string Name,
    string Email,
    string JobTitle,
    bool IsActive,
    int CurrentAllocationPercentage,
    DateOnly? HireDate,
    string? Bio);