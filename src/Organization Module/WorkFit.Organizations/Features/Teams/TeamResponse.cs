namespace WorkFit.Organizations.Features.Teams;

public sealed record TeamResponse(
    Guid Id,
    Guid DepartmentId,
    string Name,
    Guid? LeadUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
