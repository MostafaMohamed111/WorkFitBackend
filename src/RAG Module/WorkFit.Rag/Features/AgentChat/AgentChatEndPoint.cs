using FastEndpoints;
using Microsoft.AspNetCore.Http;
using WorkFit.Rag.Contracts.Agent;

namespace WorkFit.Rag.Features.AgentChat;

public sealed class AgentChatEndPoint : Endpoint<AgentChatRequest, AgentChatResponse>
{
    private readonly IAgentChatService _agentChat;

    public AgentChatEndPoint(IAgentChatService agentChat) => _agentChat = agentChat;

    public override void Configure()
    {
        Post("/api/agent/chat");
        Options(x => x.WithTags("Agent", "Recommendations"));
        Roles("TeamLeader", "OrganizationOwner", "Admin", "SuperAdmin", "Employee");
    }

    public override async Task HandleAsync(
        AgentChatRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _agentChat.RespondAsync(request, cancellationToken);
        await Send.OkAsync(response, cancellationToken);
    }
}
