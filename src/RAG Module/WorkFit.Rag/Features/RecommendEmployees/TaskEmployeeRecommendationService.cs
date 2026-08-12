using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WorkFit.Engine.Contracts.AI;
using WorkFit.Rag.Contracts.Indexing;
using WorkFit.Rag.Contracts.Recommendations;
using WorkFit.Rag.CrossCutting;
using WorkFit.Rag.Domain;
using WorkFit.Rag.Infrastructure.Options;
using WorkFit.Rag.Infrastructure.Qdrant;
using WorkFit.TalentManagement.Contracts.Indexing;

namespace WorkFit.Rag.Features.RecommendEmployees;

internal sealed class TaskEmployeeRecommendationService(
    IEmbeddingClient embeddingClient,
    IChatCompletionClient chatClient,
    IQdrantRestClient qdrant,
    IEmployeeIndexingSnapshotService employeeSnapshots,
    IEmployeeProfileIndexingService employeeIndexingService,
    IOptions<QdrantOptions> qdrantOptions,
    IOptions<RagRecommendationOptions> recommendationOptions,
    ILogger<TaskEmployeeRecommendationService> logger) : ITaskEmployeeRecommendationService
{
    public async Task<TaskEmployeeRecommendationResponse> RecommendAsync(
        TaskRecommendationContext context,
        CancellationToken cancellationToken = default)
    {
        Validate(context);
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
        await EnsureEmployeeProfilesIndexedAsync(context.OrganizationId, cancellationToken);
        var employeeFilter = new QdrantFilter(
        [
            new QdrantCondition("organizationId", new { value = context.OrganizationId }),
            new QdrantCondition("status", new { value = context.RequiredEmployeeStatus }),
            new QdrantCondition("availableAllocation", Range: new { gte = context.RequestedAllocation })
        ]);
        var employeeResults = await qdrant.SearchAsync(
            qdrantOptions.Value.EmployeeProfilesCollection,
            vector,
            Math.Max(context.ResultLimit, options.RetrievalLimit),
            employeeFilter,
            cancellationToken);

        var candidates = employeeResults
            .Select(result => (Result: result, Employee: QdrantPayloadReader.Employee(result.Payload)))
            .Where(item => item.Employee is not null)
            .Select(item => BuildCandidate(item.Result.Score, item.Employee!, context.RequiredSkills))
            .GroupBy(candidate => candidate.Employee.EmployeeProfileId)
            .Select(group => group.OrderByDescending(candidate => candidate.SemanticScore).First())
            .ToList();

        if (candidates.Count == 0)
        {
            return Empty(context);
        }

        await ApplyHistoricalPerformanceAsync(candidates, context, vector, cancellationToken);
        var llmEvaluations = await EvaluateWithLlmAsync(context, candidates, cancellationToken);

        foreach (var candidate in candidates)
        {
            if (llmEvaluations.TryGetValue(candidate.Employee.EmployeeProfileId, out var evaluation))
            {
                candidate.LlmEfficiencyScore = evaluation.Score;
                candidate.Reasoning = evaluation.Explanation;
            }
            else
            {
                candidate.LlmEfficiencyScore = DeterministicEfficiency(candidate);
                candidate.Reasoning = GroundedFallback(candidate);
            }

            candidate.FinalScore = RecommendationScoring.WeightedScore(
                candidate.SemanticScore,
                candidate.SkillScore,
                candidate.PerformanceScore,
                candidate.LlmEfficiencyScore,
                options.SemanticWeight,
                options.SkillWeight,
                options.PerformanceWeight,
                options.LlmWeight);
        }

        var ranked = candidates
            .OrderByDescending(candidate => candidate.FinalScore)
            .ThenByDescending(candidate => candidate.SkillScore)
            .ThenBy(candidate => candidate.Employee.EmployeeProfileId)
            .Take(context.ResultLimit)
            .Select((candidate, index) => new RankedEmployeeRecommendation(
                index + 1,
                candidate.Employee.EmployeeProfileId,
                candidate.Employee.EmployeeName,
                candidate.Employee.AvailableAllocation,
                candidate.SemanticScore,
                candidate.SkillScore,
                candidate.PerformanceScore,
                candidate.LlmEfficiencyScore,
                candidate.FinalScore,
                candidate.MatchedSkills,
                candidate.MissingSkills,
                candidate.Reasoning))
            .ToArray();

        return new TaskEmployeeRecommendationResponse(
            context.TaskId, context.ProjectId, context.OrganizationId, ranked);
    }

    private async Task EnsureEmployeeProfilesIndexedAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var employees = await employeeSnapshots.GetOrganizationEmployeesAsync(organizationId, cancellationToken);
        foreach (var employee in employees)
        {
            if (EmployeeIndexDocumentMapper.IsRemoved(employee))
                await employeeIndexingService.DeleteAsync(employee.EmployeeProfileId, cancellationToken);
            else
                await employeeIndexingService.UpsertAsync(EmployeeIndexDocumentMapper.Map(employee), cancellationToken);
        }
    }

    private async Task ApplyHistoricalPerformanceAsync(
        List<Candidate> candidates,
        TaskRecommendationContext context,
        ReadOnlyMemory<float> vector,
        CancellationToken cancellationToken)
    {
        var limit = recommendationOptions.Value.HistoricalTaskLimit;
        if (limit <= 0)
        {
            return;
        }

        var filter = new QdrantFilter(
        [
            new QdrantCondition("organizationId", new { value = context.OrganizationId })
        ]);
        var history = await qdrant.SearchAsync(
            qdrantOptions.Value.ProjectTasksCollection, vector, limit, filter, cancellationToken);
        var outcomes = history
            .Select(result => QdrantPayloadReader.ProjectTask(result.Payload))
            .Where(task => task is not null && task.TaskId != context.TaskId)
            .SelectMany(task => task!.EmployeeOutcomes ?? [])
            .GroupBy(outcome => outcome.EmployeeProfileId)
            .ToDictionary(
                group => group.Key,
                group => RecommendationScoring.Clamp(group.Average(outcome => outcome.PerformanceScore)));

        foreach (var candidate in candidates)
        {
            var profilePerformance = RecommendationScoring.Clamp(candidate.Employee.PerformanceScore);
            candidate.PerformanceScore = outcomes.TryGetValue(candidate.Employee.EmployeeProfileId, out var historical)
                ? RecommendationScoring.Clamp(profilePerformance * 0.7 + historical * 0.3)
                : profilePerformance;
        }
    }

    private async Task<IReadOnlyDictionary<Guid, LlmEvaluation>> EvaluateWithLlmAsync(
        TaskRecommendationContext context,
        IReadOnlyList<Candidate> candidates,
        CancellationToken cancellationToken)
    {
        var candidateFacts = candidates.Select(candidate => new
        {
            candidate.Employee.EmployeeProfileId,
            candidate.Employee.EmployeeName,
            candidate.Employee.AvailableAllocation,
            candidate.SemanticScore,
            candidate.SkillScore,
            candidate.PerformanceScore,
            matchedSkills = candidate.MatchedSkills,
            missingSkills = candidate.MissingSkills
        });
        var userContent = JsonSerializer.Serialize(new
        {
            task = new
            {
                context.TaskTitle,
                context.TaskDescription,
                context.ProjectName,
                context.ProjectDescription,
                context.OrganizationName,
                context.Prompt,
                context.RequestedAllocation,
                requiredSkills = context.RequiredSkills
            },
            candidates = candidateFacts
        });
        const string system = "Evaluate employee efficiency for the supplied task using only the supplied facts. " +
            "Return JSON only: {\"candidates\":[{\"employeeProfileId\":\"guid\",\"efficiencyScore\":0.0," +
            "\"explanation\":\"grounded concise explanation\"}]}. Scores must be from 0 to 1. " +
            "Do not invent experience, skills, availability, or performance.";

        try
        {
            var response = await chatClient.SendAsync(
                new ChatCompletionRequest(
                    recommendationOptions.Value.ChatModel,
                    [new ChatMessage("system", system), new ChatMessage("user", userContent)],
                    Temperature: 0,
                    ResponseFormatJson: true,
                    MaxTokens: recommendationOptions.Value.ChatMaxTokens),
                cancellationToken);
            return ParseLlmResponse(response.Content, candidates);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "RAG chat evaluation failed; deterministic candidate scoring will be used.");
            return new Dictionary<Guid, LlmEvaluation>();
        }
    }

    private static IReadOnlyDictionary<Guid, LlmEvaluation> ParseLlmResponse(
        string content,
        IReadOnlyList<Candidate> candidates)
    {
        var allowedIds = candidates.Select(candidate => candidate.Employee.EmployeeProfileId).ToHashSet();
        var parsed = new Dictionary<Guid, LlmEvaluation>();
        try
        {
            using var document = JsonDocument.Parse(content);
            if (!document.RootElement.TryGetProperty("candidates", out var items) ||
                items.ValueKind != JsonValueKind.Array)
            {
                return parsed;
            }

            foreach (var item in items.EnumerateArray())
            {
                if (!TryGuid(item, "employeeProfileId", out var id) || !allowedIds.Contains(id))
                {
                    continue;
                }

                var score = TryScore(item, "efficiencyScore", out var rawScore)
                    ? RecommendationScoring.Clamp(rawScore > 1 && rawScore <= 100 ? rawScore / 100 : rawScore)
                    : 0;
                var explanation = item.TryGetProperty("explanation", out var explanationElement) &&
                    explanationElement.ValueKind == JsonValueKind.String
                    ? explanationElement.GetString()?.Trim()
                    : null;
                if (string.IsNullOrWhiteSpace(explanation))
                {
                    explanation = "No grounded model explanation was returned; score was validated from model output.";
                }

                parsed[id] = new LlmEvaluation(score, explanation);
            }
        }
        catch (JsonException)
        {
            return parsed;
        }

        return parsed;
    }

    private static Candidate BuildCandidate(
        double qdrantScore,
        IndexedEmployeeProfile employee,
        IReadOnlyList<RequiredSkill> requiredSkills)
    {
        var skillScore = RecommendationScoring.CalculateSkillScore(
            requiredSkills, employee.Skills ?? [], out var matched, out var missing);
        return new Candidate(
            employee,
            RecommendationScoring.NormalizeCosine(qdrantScore),
            skillScore,
            RecommendationScoring.Clamp(employee.PerformanceScore),
            matched,
            missing);
    }

    private static string BuildQuery(TaskRecommendationContext context) =>
        $"Task: {context.TaskTitle}\nTask description: {context.TaskDescription}\n" +
        $"Project: {context.ProjectName}\nProject description: {context.ProjectDescription}\n" +
        $"Required skills: {string.Join(", ", context.RequiredSkills.Select(skill => $"{skill.Name} level {skill.RequiredLevel:0.##}"))}\n" +
        $"Requested allocation: {context.RequestedAllocation:0.##}\nAdditional prompt: {context.Prompt}";

    private static double DeterministicEfficiency(Candidate candidate) => RecommendationScoring.WeightedScore(
        candidate.SemanticScore, candidate.SkillScore, candidate.PerformanceScore, 0,
        0.4, 0.35, 0.25, 0);

    private static string GroundedFallback(Candidate candidate)
    {
        var matched = candidate.MatchedSkills.Count == 0
            ? "no explicitly matched required skills"
            : $"matched skills: {string.Join(", ", candidate.MatchedSkills)}";
        var missing = candidate.MissingSkills.Count == 0
            ? "no required skill gaps in indexed data"
            : $"missing skills: {string.Join(", ", candidate.MissingSkills)}";
        return $"Based on indexed data: {matched}; {missing}; available allocation " +
            $"{candidate.Employee.AvailableAllocation:0.##}; performance score {candidate.PerformanceScore:0.###}.";
    }

    private static bool TryGuid(JsonElement item, string property, out Guid value)
    {
        value = Guid.Empty;
        return item.TryGetProperty(property, out var element) &&
            Guid.TryParse(element.ToString(), out value);
    }

    private static bool TryScore(JsonElement item, string property, out double value)
    {
        value = 0;
        if (!item.TryGetProperty(property, out var element))
        {
            return false;
        }

        return element.ValueKind == JsonValueKind.Number
            ? element.TryGetDouble(out value)
            : element.ValueKind == JsonValueKind.String &&
              double.TryParse(element.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static void Validate(TaskRecommendationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.TaskId == Guid.Empty || context.ProjectId == Guid.Empty || context.OrganizationId == Guid.Empty)
        {
            throw new ArgumentException("Task, project, and organization IDs are required.", nameof(context));
        }

        if (string.IsNullOrWhiteSpace(context.TaskTitle) || string.IsNullOrWhiteSpace(context.ProjectName))
        {
            throw new ArgumentException("Task title and project name are required.", nameof(context));
        }

        if (context.RequiredSkills is null || context.RequiredSkills.Any(skill =>
                string.IsNullOrWhiteSpace(skill.Name) ||
                !double.IsFinite(skill.RequiredLevel) || skill.RequiredLevel < 0 ||
                !double.IsFinite(skill.Weight) || skill.Weight < 0))
        {
            throw new ArgumentException("Required skills must have names and non-negative finite levels and weights.", nameof(context));
        }

        if (string.IsNullOrWhiteSpace(context.RequiredEmployeeStatus))
        {
            throw new ArgumentException("Required employee status is required.", nameof(context));
        }

        if (!double.IsFinite(context.RequestedAllocation) || context.RequestedAllocation < 0 || context.ResultLimit is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(context), "Allocation must be non-negative and result limit must be 1-100.");
        }
    }

    private static TaskEmployeeRecommendationResponse Empty(TaskRecommendationContext context) =>
        new(context.TaskId, context.ProjectId, context.OrganizationId, []);

    private sealed class Candidate(
        IndexedEmployeeProfile employee,
        double semanticScore,
        double skillScore,
        double performanceScore,
        IReadOnlyList<string> matchedSkills,
        IReadOnlyList<string> missingSkills)
    {
        public IndexedEmployeeProfile Employee { get; } = employee;
        public double SemanticScore { get; } = semanticScore;
        public double SkillScore { get; } = skillScore;
        public double PerformanceScore { get; set; } = performanceScore;
        public IReadOnlyList<string> MatchedSkills { get; } = matchedSkills;
        public IReadOnlyList<string> MissingSkills { get; } = missingSkills;
        public double LlmEfficiencyScore { get; set; }
        public double FinalScore { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }

    private sealed record LlmEvaluation(double Score, string Explanation);
}
