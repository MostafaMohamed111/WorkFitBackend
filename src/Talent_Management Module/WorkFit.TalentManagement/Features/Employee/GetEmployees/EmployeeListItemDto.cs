namespace WorkFit.TalentManagement.Features.Employee.GetEmployees;

public sealed record EmployeeListItemDto(
    Guid Id,
    string Name,
    string Email,
    string JobTitle,
    bool IsActive,
    int CurrentAllocationPercentage);