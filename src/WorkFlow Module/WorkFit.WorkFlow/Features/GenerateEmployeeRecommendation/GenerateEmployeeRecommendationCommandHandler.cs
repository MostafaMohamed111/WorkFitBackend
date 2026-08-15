using System.Globalization;
using WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;
using WorkFit.Rag.Contracts.Recommendations;
using WorkFit.Recommendations.Contracts.CreateRecommendationService;
using WorkFit.Skills.Contracts.SkillLookUp;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Contracts.Indexing;

namespace WorkFit.WorkFlow.Features.GenerateEmployeeRecommendation;

public sealed class GenerateEmployeeRecommendationCommandHandler
    : IRequestHandler<GenerateEmployeeRecommendationCommand, GenerateEmployeeRecommendationResponse>
{
    private const int DefaultResultLimit = 10;

    private readonly ICurrentUserContext _currentUserContext;
    private readonly ITaskLookUpService _taskLookUpService;
    private readonly ITaskEmployeeRecommendationService _taskEmployeeRecommendationService;
    private readonly ICreateRecommendationService _createRecommendationService;
    private readonly ISkillLookUpService _skillLookUpService;
    private readonly IEmployeeIndexingSnapshotService _employeeIndexingSnapshotService;

    public GenerateEmployeeRecommendationCommandHandler(
        ICurrentUserContext currentUserContext,
        ITaskLookUpService taskLookUpService,
        ITaskEmployeeRecommendationService taskEmployeeRecommendationService,
        ICreateRecommendationService createRecommendationService,
        ISkillLookUpService skillLookUpService,
        IEmployeeIndexingSnapshotService employeeIndexingSnapshotService)
    {
        _currentUserContext = currentUserContext;
        _taskLookUpService = taskLookUpService;
        _taskEmployeeRecommendationService = taskEmployeeRecommendationService;
        _createRecommendationService = createRecommendationService;
        _skillLookUpService = skillLookUpService;
        _employeeIndexingSnapshotService = employeeIndexingSnapshotService;
    }

    public async Task<GenerateEmployeeRecommendationResponse> Handle(
        GenerateEmployeeRecommendationCommand command,
        CancellationToken cancellationToken = default)
    {
        var resultLimit = command.ResultLimit ?? DefaultResultLimit;
        if (resultLimit is < 1 or > 100)
        {
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "INVALID_RECOMMENDATION_RESULT_LIMIT",
                $"Recommendation result limit '{resultLimit}' is outside the supported range.",
                "Result limit must be between 1 and 100.");
        }

        if (command.Prompt?.Length > 2000)
        {
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "RECOMMENDATION_PROMPT_TOO_LONG",
                $"Recommendation prompt length '{command.Prompt.Length}' exceeds the supported limit.",
                "Prompt cannot exceed 2000 characters.");
        }

        var currentUserId = _currentUserContext.GetUserId(cancellationToken);
        var task = await _taskLookUpService.GetRecommendationContextAsync(
            command.TaskId,
            cancellationToken);

        var callerRoles = _currentUserContext.GetRoles(cancellationToken);
        if (task.TeamLeaderId != currentUserId &&
            !callerRoles.Contains("OrganizationOwner") &&
            !callerRoles.Contains("Admin") &&
            !callerRoles.Contains("SuperAdmin"))
        {
            throw new ForbiddenAccessException(
                ModuleMarker.ModuleName,
                "Task recommendation",
                "Only the project's team leader or organization owner can generate recommendations for this task.");
        }

        if (!task.IsActive || task.IsDeleted || task.AssignedEmployeeId.HasValue ||
            string.Equals(task.Status, "Done", StringComparison.OrdinalIgnoreCase))
        {
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "TASK_NOT_ELIGIBLE_FOR_RECOMMENDATION",
                $"Task '{task.Id}' is not active, is deleted, or is already assigned.",
                "Recommendations can only be generated for active, unassigned tasks.");
        }

        if (!string.Equals(task.ProjectStatus, "Active", StringComparison.OrdinalIgnoreCase))
        {
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "PROJECT_NOT_ELIGIBLE_FOR_RECOMMENDATION",
                $"Project '{task.ProjectId}' has status '{task.ProjectStatus}'.",
                "Recommendations can only be generated for active projects.");
        }

        var skillNames = (await _skillLookUpService.GetSkillsByIdsAsync(
                task.ProjectRequiredSkills.Select(skill => skill.SkillId)))
            .ToDictionary(skill => skill.SkillId, skill => skill.Name);

        var requiredSkills = task.ProjectRequiredSkills
            .OrderBy(skill => skill.Priority)
            .Select(skill => new RequiredSkill(
                skill.SkillId,
                skillNames.GetValueOrDefault(skill.SkillId, skill.SkillId.ToString("D")),
                ParseRequiredLevel(skill.Level),
                PriorityWeight(skill.Priority)))
            .ToArray();

        var ragResponse = await _taskEmployeeRecommendationService.RecommendAsync(
            new TaskRecommendationContext(
                task.Id,
                task.Title,
                task.Description,
                task.ProjectId,
                task.ProjectName,
                task.ProjectDescription,
                task.OrganizationId,
                null,
                string.IsNullOrWhiteSpace(command.Prompt) ? null : command.Prompt.Trim(),
                task.AllocationPercentage,
                requiredSkills,
                resultLimit),
            cancellationToken);

        var liveEmployees = new List<(RankedEmployeeRecommendation Candidate, EmployeeIndexingSnapshot? Snapshot)>();
        foreach (var candidate in ragResponse.Employees)
        {
            liveEmployees.Add((candidate, await _employeeIndexingSnapshotService.GetEmployeeAsync(
                candidate.EmployeeProfileId,
                cancellationToken)));
        }

        var eligibleEmployees = liveEmployees
            .Where(employee =>
                employee.Snapshot is not null &&
                employee.Snapshot.OrganizationId == task.OrganizationId &&
                string.Equals(employee.Snapshot.Status, "Active", StringComparison.OrdinalIgnoreCase) &&
                Math.Max(0, 100 - employee.Snapshot.CurrentAllocationPercentage) >= task.AllocationPercentage)
            .Select((employee, index) => new
            {
                Candidate = employee.Candidate,
                Rank = index + 1,
                AvailableAllocation = Math.Max(0, 100 - employee.Snapshot!.CurrentAllocationPercentage)
            })
            .ToArray();

        if (eligibleEmployees.Length == 0)
        {
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "NO_ELIGIBLE_EMPLOYEE_RECOMMENDATIONS",
                $"No eligible employees were found for task '{task.Id}'.",
                "No eligible employees were found for this task.");
        }

        var persisted = await _createRecommendationService.CreateAsync(
            new CreateRecommendationDto(
                task.Id,
                requiredSkills.Select(skill => skill.SkillId!.Value).ToArray(),
                eligibleEmployees.Select(employee => new RankedRecommendationCandidateDto(
                    employee.Candidate.EmployeeProfileId,
                    employee.Rank,
                    ToPercentage(employee.Candidate.FinalScore),
                    [
                        new RecommendationScoreComponentDto("Semantic", ToPercentage(employee.Candidate.SemanticScore)),
                        new RecommendationScoreComponentDto("Skill", ToPercentage(employee.Candidate.SkillScore)),
                        new RecommendationScoreComponentDto("Performance", ToPercentage(employee.Candidate.PerformanceScore)),
                        new RecommendationScoreComponentDto("LlmEfficiency", ToPercentage(employee.Candidate.LlmEfficiencyScore))
                    ],
                    employee.Candidate.GroundedReasoning)).ToArray()),
            cancellationToken);

        var eligibleCandidates = eligibleEmployees.ToDictionary(employee => employee.Candidate.EmployeeProfileId);
        return new GenerateEmployeeRecommendationResponse(
            persisted.RecommendationId,
            persisted.TaskId,
            ragResponse.ProjectId,
            ragResponse.OrganizationId,
            persisted.CreatedBy,
            persisted.CreatedAt,
            persisted.Candidates
                .OrderBy(candidate => candidate.Rank)
                .Select(candidate =>
                {
                    var eligible = eligibleCandidates[candidate.EmployeeId];
                    var ranked = eligible.Candidate;
                    return new EmployeeRecommendationDto(
                        candidate.CandidateId,
                        candidate.EmployeeId,
                        ranked.EmployeeName,
                        candidate.Rank,
                        candidate.Score,
                        eligible.AvailableAllocation,
                        candidate.ScoreBreakdown
                            .Select(component => new RecommendationScoreDto(component.Name, component.Score))
                            .ToArray(),
                        ranked.MatchedSkills,
                        ranked.MissingSkills,
                        candidate.Reasoning);
                })
                .ToArray());
    }

    private static double ParseRequiredLevel(string level)
    {
        if (double.TryParse(level, NumberStyles.Float, CultureInfo.InvariantCulture, out var numericLevel) &&
            double.IsFinite(numericLevel) && numericLevel >= 0)
        {
            return numericLevel;
        }

        return level.Trim().ToLowerInvariant() switch
        {
            "beginner" => 0.33,
            "proficient" => 0.66,
            "expert" => 1,
            _ => 0
        };
    }

    private static double PriorityWeight(int priority) => priority switch
    {
        <= 1 => 1,
        2 => 0.8,
        3 => 0.6,
        4 => 0.4,
        _ => 0.2
    };

    private static decimal ToPercentage(double score)
    {
        if (!double.IsFinite(score))
        {
            return 0;
        }

        return Math.Round((decimal)Math.Clamp(score, 0, 1) * 100m, 2);
    }
}
