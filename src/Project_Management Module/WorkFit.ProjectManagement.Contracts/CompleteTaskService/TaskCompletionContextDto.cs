namespace WorkFit.ProjectManagement.Contracts.CompleteTaskService;

public sealed record TaskCompletionContextDto(
    Guid TaskId,
    Guid ProjectId,
    Guid OrganizationId,
    Guid? AssignedEmployeeId,
    Guid? TeamLeaderId,
    string RepositoryName,
    string BranchName,
    int AllocationPercentage);