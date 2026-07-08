

namespace WorkFit.TalentManagement.Contracts.WriteServices.CreateEmployee;
public sealed record EmployeeDetails(
    Guid organizationId,
    Guid userId,
    string email,
    string name,
    string jobTitle,
    DateOnly? hireDate = null
);