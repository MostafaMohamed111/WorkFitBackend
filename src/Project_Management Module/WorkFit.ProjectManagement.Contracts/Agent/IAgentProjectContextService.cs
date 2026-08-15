namespace WorkFit.ProjectManagement.Contracts.Agent;

public interface IAgentProjectContextService
{
    Task<IReadOnlyList<AgentProjectContextDto>> GetVisibleProjectsAsync(
        Guid? projectId,
        CancellationToken cancellationToken = default);
}

public sealed record AgentProjectContextDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Description,
    string Status,
    Guid? TeamLeaderId,
    IReadOnlyList<Guid> EmployeeIds,
    IReadOnlyList<AgentProjectTaskDto> Tasks);

public sealed record AgentProjectTaskDto(
    Guid Id,
    string Title,
    string Status,
    string Priority,
    Guid? AssignedEmployeeId,
    int AllocationPercentage,
    DateOnly? DueDate);
