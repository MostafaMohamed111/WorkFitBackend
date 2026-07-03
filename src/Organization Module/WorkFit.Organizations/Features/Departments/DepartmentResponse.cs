namespace WorkFit.Organizations.Features.Departments;

public sealed record DepartmentResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
