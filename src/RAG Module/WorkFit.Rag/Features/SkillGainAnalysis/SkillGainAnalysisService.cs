using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkFit.Engine.Contracts.AI;
using WorkFit.Rag.Contracts.SkillGainAnalysis;
using WorkFit.Rag.Infrastructure.Options;
using WorkFit.Rag.Infrastructure.Qdrant;

namespace WorkFit.Rag.Features.SkillGainAnalysis;

internal sealed class SkillGainAnalysisService(
    IEmbeddingClient embeddingClient,
    IChatCompletionClient chatClient,
    IQdrantRestClient qdrant,
    IOptions<QdrantOptions> qdrantOptions,
    IOptions<RagRecommendationOptions> recommendationOptions,
    ILogger<SkillGainAnalysisService> logger) : ISkillGainAnalysisService
{
    private const int VectorRetrievalLimit = 10;

    public async Task<SkillGainAnalysisResponse> AnalyzeAsync(
        SkillGainAnalysisContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.TaskId == Guid.Empty || context.ProjectId == Guid.Empty || context.OrganizationId == Guid.Empty)
        {
            throw new ArgumentException("Task, project, and organization IDs are required.", nameof(context));
        }

        var options = recommendationOptions.Value;
        var query = BuildQuery(context);
        var embedding = await embeddingClient.EmbedAsync(
            new EmbeddingRequest(
                options.EmbeddingModel,
                [query],
                EmbeddingTaskType.RetrievalQuery,
                QdrantOptions.VectorSize),
            cancellationToken);
        if (embedding.Vectors.Count != 1)
        {
            throw new InvalidOperationException("Embedding provider returned an unexpected vector count.");
        }

        var vector = embedding.Vectors[0];
        var organizationFilter = new QdrantFilter(
        [
            new QdrantCondition("organizationId", new { value = context.OrganizationId })
        ]);

        var employeeVectorResults = await qdrant.SearchAsync(
            qdrantOptions.Value.EmployeeProfilesCollection,
            vector,
            VectorRetrievalLimit,
            organizationFilter,
            cancellationToken);
        var taskVectorResults = await qdrant.SearchAsync(
            qdrantOptions.Value.ProjectTasksCollection,
            vector,
            VectorRetrievalLimit,
            organizationFilter,
            cancellationToken);

        var employeeFacts = employeeVectorResults
            .Select(result => QdrantPayloadReader.Employee(result.Payload))
            .Where(employee => employee is not null)
            .Select(employee => new
            {
                employee!.EmployeeProfileId,
                employee.EmployeeName,
                employee.Status,
                employee.PerformanceScore,
                employee.AvailableAllocation,
                skills = employee.Skills?.Select(skill => new { skill.Name, skill.Level }) ?? []
            });
        var taskFacts = taskVectorResults
            .Select(result => QdrantPayloadReader.ProjectTask(result.Payload))
            .Where(task => task is not null)
            .Select(task => new
            {
                task!.TaskId,
                outcomes = task.EmployeeOutcomes?
                    .Where(outcome => outcome.EmployeeProfileId == context.EmployeeProfileId)
                    .Select(outcome => new { outcome.PerformanceScore }) ?? []
            });

        try
        {
            var userContent = JsonSerializer.Serialize(new
            {
                task = new
                {
                    context.TaskId,
                    context.TaskTitle,
                    context.TaskDescription,
                    context.ProjectName,
                    context.ProjectDescription,
                    requiredSkills = context.RequiredSkills
                },
                employee = new
                {
                    context.EmployeeProfileId,
                    context.EmployeeName,
                    context.EmployeeJobTitle,
                    skills = context.EmployeeSkills
                },
                codeReview = context.CodeReview,
                vectorData = new
                {
                    employeeProfiles = employeeFacts,
                    projectTasks = taskFacts
                }
            });
            const string system = "You analyze a completed task to determine the skill growth of the assigned employee. " +
                "Use ONLY the supplied facts: the task, the employee profile with skill confidence scores (0-100), " +
                "the AI code review of the employee's changes, and the vector database facts about the employee and tasks. " +
                "Return JSON only: {\"skillChanges\":[{\"skillName\":\"string\",\"newScore\":0," +
                "\"reasoning\":\"grounded concise explanation\"}],\"newSkills\":[{\"skillName\":\"string\"," +
                "\"newScore\":0,\"reasoning\":\"grounded concise explanation\"}]}. " +
                "skillChanges must reference skills the employee already has (match by name); " +
                "newSkills must be skills not present in the employee profile that the task or code review demonstrates. " +
                "newScore must be an integer from 0 to 100 representing the confidence score after the task. " +
                "Do not invent skills, experience, or evidence not present in the supplied facts.";

            var response = await chatClient.SendAsync(
                new ChatCompletionRequest(
                    options.ChatModel,
                    [new ChatMessage("system", system), new ChatMessage("user", userContent)],
                    Temperature: 0,
                    ResponseFormatJson: true,
                    MaxTokens: options.ChatMaxTokens),
                cancellationToken);

            return ParseLlmResponse(response.Content, context.EmployeeSkills);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Skill gain analysis failed for task {TaskId} and employee {EmployeeProfileId}; returning an empty analysis.",
                context.TaskId,
                context.EmployeeProfileId);
            return new SkillGainAnalysisResponse([], []);
        }
    }

    private static SkillGainAnalysisResponse ParseLlmResponse(
        string content,
        IReadOnlyList<EmployeeSkillGainInput> employeeSkills)
    {
        var skillChanges = new List<SkillGainChange>();
        var newSkills = new List<NewSkillGain>();
        try
        {
            using var document = JsonDocument.Parse(content);
            if (!document.RootElement.TryGetProperty("skillChanges", out var changes) ||
                changes.ValueKind != JsonValueKind.Array)
            {
                return new SkillGainAnalysisResponse([], []);
            }

            foreach (var item in changes.EnumerateArray())
            {
                if (!TryString(item, "skillName", out var skillName) ||
                    !TryScore(item, "newScore", out var newScore) ||
                    !TryString(item, "reasoning", out var reasoning))
                {
                    continue;
                }

                var existing = employeeSkills.FirstOrDefault(skill =>
                    string.Equals(skill.Name, skillName, StringComparison.OrdinalIgnoreCase));
                if (existing is not null)
                {
                    skillChanges.Add(new SkillGainChange(
                        existing.SkillId,
                        existing.Name,
                        existing.ConfidenceScore,
                        ClampScore(newScore),
                        reasoning));
                }
                else
                {
                    newSkills.Add(new NewSkillGain(skillName, ClampScore(newScore), reasoning));
                }
            }
        }
        catch (JsonException)
        {
            return new SkillGainAnalysisResponse([], []);
        }

        return new SkillGainAnalysisResponse(skillChanges, newSkills);
    }

    private static string BuildQuery(SkillGainAnalysisContext context) =>
        $"Completed task: {context.TaskTitle}\nTask description: {context.TaskDescription}\n" +
        $"Project: {context.ProjectName}\nProject description: {context.ProjectDescription}\n" +
        $"Required skills: {string.Join(", ", context.RequiredSkills.Select(skill => $"{skill.Name} level {skill.RequiredLevel:0.##}"))}\n" +
        $"Employee: {context.EmployeeName}\nEmployee skills: {string.Join(", ", context.EmployeeSkills.Select(skill => $"{skill.Name} ({skill.ConfidenceScore})"))}";

    private static bool TryString(JsonElement item, string property, out string value)
    {
        value = string.Empty;
        if (!item.TryGetProperty(property, out var element) || element.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = element.GetString()?.Trim() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool TryScore(JsonElement item, string property, out int value)
    {
        value = 0;
        if (!item.TryGetProperty(property, out var element))
        {
            return false;
        }

        return element.ValueKind == JsonValueKind.Number
            ? element.TryGetInt32(out value)
            : element.ValueKind == JsonValueKind.String &&
              int.TryParse(element.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    private static int ClampScore(int score) => Math.Clamp(score, 0, 100);
}