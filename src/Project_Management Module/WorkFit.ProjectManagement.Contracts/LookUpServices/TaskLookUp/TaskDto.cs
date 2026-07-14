namespace WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;

public sealed record TaskDto(
    Guid Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    Guid ProjectId,
    Guid TeamLeadId,
    Guid AssignedToUserId,
    string Status
);