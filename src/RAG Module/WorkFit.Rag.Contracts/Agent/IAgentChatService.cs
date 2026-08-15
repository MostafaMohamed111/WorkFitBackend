namespace WorkFit.Rag.Contracts.Agent;

public interface IAgentChatService
{
    Task<AgentChatResponse> RespondAsync(
        AgentChatRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record AgentChatRequest(
    Guid? TaskId,
    Guid? ProjectId,
    string Prompt,
    int? ResultLimit);

public sealed record AgentChatResponse(
    string Reply,
    Guid? RecommendationId,
    Guid? TaskId,
    IReadOnlyList<AgentChatCandidateDto> Candidates);

public sealed record AgentChatCandidateDto(
    Guid EmployeeId,
    string EmployeeName,
    int Rank,
    decimal Score,
    double AvailableAllocation,
    IReadOnlyList<string> MatchedSkills,
    IReadOnlyList<string> MissingSkills,
    string Reasoning);
