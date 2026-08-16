using System.Text.Json;
using WorkFit.Engine.Contracts.AI;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.ProjectManagement.Contracts.Agent;
using WorkFit.Rag.Contracts.Agent;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Contracts.LookUpServices;
using WorkFit.WorkFlow.Features.GenerateEmployeeRecommendation;

namespace WorkFit.WorkFlow.Features.AgentChat;

public sealed class AgentChatService : IAgentChatService
{
    private readonly IMediator _mediator;
    private readonly IAgentProjectContextService _projects;
    private readonly IEmployeeLookUpService _employees;
    private readonly IChatCompletionClient _chat;
    private readonly ICurrentUserContext _currentUser;
    private readonly IGetOrganizationIdService _organizations;

    public AgentChatService(
        IMediator mediator,
        IAgentProjectContextService projects,
        IEmployeeLookUpService employees,
        IChatCompletionClient chat,
        ICurrentUserContext currentUser,
        IGetOrganizationIdService organizations)
    {
        _mediator = mediator;
        _projects = projects;
        _employees = employees;
        _chat = chat;
        _currentUser = currentUser;
        _organizations = organizations;
    }

    public async Task<AgentChatResponse> RespondAsync(
        AgentChatRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "AGENT_PROMPT_REQUIRED",
                "The agent request prompt was empty.",
                "Enter a message for the agent.");
        }

        if (request.TaskId.HasValue && request.TaskId.Value != Guid.Empty)
        {
                var recommendation = await _mediator.Send(
                    new GenerateEmployeeRecommendationCommand(
                        request.TaskId.Value,
                        request.Prompt.Trim(),
                        request.ResultLimit),
                    cancellationToken);

                var candidates = recommendation.Candidates
                    .Select(candidate => new AgentChatCandidateDto(
                        candidate.EmployeeId,
                        candidate.EmployeeName,
                        candidate.Rank,
                        candidate.Score,
                        candidate.AvailableAllocation,
                        candidate.MatchedSkills,
                        candidate.MissingSkills,
                        candidate.Reasoning))
                    .ToArray();

                return new AgentChatResponse(
                    candidates.Length == 0
                        ? "I could not find an eligible person for this task using the current WorkFit data."
                        : $"I analyzed the selected task against indexed employee profiles and task history. The best match is {candidates[0].EmployeeName} with a score of {candidates[0].Score:0.##}%. {candidates[0].Reasoning}",
                    recommendation.RecommendationId,
                    recommendation.TaskId,
                    candidates);
            
        }

        var projects = await _projects.GetVisibleProjectsAsync(request.ProjectId, cancellationToken);
        var organizationId = await _organizations.GetOrganizationIdAsync(
            _currentUser.GetUserId(cancellationToken), cancellationToken);
        var organizationEmployees = await _employees.GetEmployeesByOrganizationIdAsync(
            organizationId, cancellationToken);
        var employees = organizationEmployees.Take(200).Select(employee => new
        {
            employee.Id,
            employee.Name,
            employee.JobTitle,
            employee.Status,
            employee.IsActive,
            employee.CurrentAllocationPercentage,
            skills = employee.Skills.Select(skill => new { skill.SkillName, skill.ConfidenceScore })
        }).ToList();

        if (projects.Count == 0 && employees.Count == 0)
        {
            return new AgentChatResponse(
                request.ProjectId.HasValue
                    ? "I could not find that project in the projects you are allowed to access."
                    : "I could not find any active projects or team members in your organization.",
                null, null, []);
        }

        var response = await _chat.SendAsync(
            new ChatCompletionRequest(
                string.Empty,
                [
                    new ChatMessage("system", "You are WorkFit AI Sidekick, an intelligent assistant for team leaders and organization owners. Answer the user's question accurately using the JSON facts supplied by WorkFit. A project's employeeIds identifies the team members working on that project; match employeeIds to employee objects by id. When asked about project members, list the names, titles, and key skills of the people working on that project. Be concise, clear, and helpful."),
                    new ChatMessage("user", $"WorkFit facts:\n{JsonSerializer.Serialize(new { projects, employees })}\n\nQuestion:\n{request.Prompt.Trim()}")
                ],
                Temperature: 0,
                MaxTokens: 1000),
            cancellationToken);

        return new AgentChatResponse(response.Content, null, null, []);
    }

    private static bool IsRecommendationStateException(FeatureException ex) =>
        ex.Code.EndsWith("NO_ELIGIBLE_EMPLOYEE_RECOMMENDATIONS", StringComparison.OrdinalIgnoreCase) ||
        ex.Code.EndsWith("TASK_NOT_ELIGIBLE_FOR_RECOMMENDATION", StringComparison.OrdinalIgnoreCase) ||
        ex.Code.EndsWith("PROJECT_NOT_ELIGIBLE_FOR_RECOMMENDATION", StringComparison.OrdinalIgnoreCase);
}
