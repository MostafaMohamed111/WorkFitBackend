using System.Globalization;
using Microsoft.Extensions.Logging;
using WorkFit.Assessments.Contracts.CreateAssessmentService;
using WorkFit.CodeReview.Contracts.GitHubCodeReview;
using WorkFit.Organizations.Contracts.OrganizationGitHub;
using WorkFit.ProjectManagement.Contracts.CompleteTaskService;
using WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;
using WorkFit.Rag.Contracts.Recommendations;
using WorkFit.Rag.Contracts.SkillGainAnalysis;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.Skills.Contracts;
using WorkFit.Skills.Contracts.SkillLookUp;
using WorkFit.TalentManagement.Contracts.LookUpServices;

namespace WorkFit.WorkFlow.Features.CompleteTask;

public sealed class TakeCompleteTaskCommandHandler
    : IRequestHandler<TakeCompleteTaskCommand, TakeCompleteTaskResponse>
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IEmployeeLookUpService _employeeLookUpService;
    private readonly ITaskLookUpService _taskLookUpService;
    private readonly ICompleteProjectTaskService _completeProjectTaskService;
    private readonly IGitHubOrganizationLoginLookupService _gitHubOrganizationLoginLookupService;
    private readonly IReviewTaskGitHub _reviewTaskGitHub;
    private readonly ISkillGainAnalysisService _skillGainAnalysisService;
    private readonly ISkillLookUpService _skillLookUpService;
    private readonly ISkillCatalog _skillCatalog;
    private readonly ICreateAssessmentService _createAssessmentService;
    private readonly ILogger<TakeCompleteTaskCommandHandler> _logger;

    public TakeCompleteTaskCommandHandler(
        ICurrentUserContext currentUserContext,
        IEmployeeLookUpService employeeLookUpService,
        ITaskLookUpService taskLookUpService,
        ICompleteProjectTaskService completeProjectTaskService,
        IGitHubOrganizationLoginLookupService gitHubOrganizationLoginLookupService,
        IReviewTaskGitHub reviewTaskGitHub,
        ISkillGainAnalysisService skillGainAnalysisService,
        ISkillLookUpService skillLookUpService,
        ISkillCatalog skillCatalog,
        ICreateAssessmentService createAssessmentService,
        ILogger<TakeCompleteTaskCommandHandler> logger)
    {
        _currentUserContext = currentUserContext;
        _employeeLookUpService = employeeLookUpService;
        _taskLookUpService = taskLookUpService;
        _completeProjectTaskService = completeProjectTaskService;
        _gitHubOrganizationLoginLookupService = gitHubOrganizationLoginLookupService;
        _reviewTaskGitHub = reviewTaskGitHub;
        _skillGainAnalysisService = skillGainAnalysisService;
        _skillLookUpService = skillLookUpService;
        _skillCatalog = skillCatalog;
        _createAssessmentService = createAssessmentService;
        _logger = logger;
    }

    public async Task<TakeCompleteTaskResponse> Handle(
        TakeCompleteTaskCommand command,
        CancellationToken cancellationToken = default)
    {
        var callerUserId = _currentUserContext.GetUserId(cancellationToken);
        var callerEmployee = await _employeeLookUpService.GetEmployeeByUserIdAsync(callerUserId, cancellationToken);
        if (callerEmployee is null)
        {
            throw new ForbiddenAccessException(
                ModuleMarker.ModuleName,
                "Task completion",
                "Only an employee can complete a task with code review.");
        }

        var task = await _taskLookUpService.GetRecommendationContextAsync(command.TaskId, cancellationToken);

        if (task.AssignedEmployeeId != callerEmployee.Id)
        {
            throw new ForbiddenAccessException(
                ModuleMarker.ModuleName,
                "Task completion",
                "Only the employee assigned to the task can complete it with code review.");
        }

        var completion = await _completeProjectTaskService.CompleteTaskAsync(command.TaskId, cancellationToken);

        var organizationLogin = await _gitHubOrganizationLoginLookupService
            .GetGitHubOrganizationLoginAsync(completion.OrganizationId, cancellationToken);
        if (string.IsNullOrWhiteSpace(organizationLogin))
        {
            throw new InvalidOperationException("The organization is not connected to GitHub.");
        }

        var codeReview = await _reviewTaskGitHub.ReviewTaskAsync(
            completion.TaskId,
            completion.AssignedEmployeeId,
            organizationLogin,
            completion.RepositoryName,
            completion.BranchName,
            null,
            null,
            cancellationToken);

        var employee = await _employeeLookUpService.GetEmployeeByIdAsync(callerEmployee.Id, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, "EmployeeProfile", callerEmployee.Id);

        var skillGainAnalysis = await AnalyzeSkillGainsAsync(task, employee, codeReview, cancellationToken);

        var assessmentId = await CreateTeamLeadAssessmentAsync(
            task,
            employee,
            skillGainAnalysis,
            cancellationToken);

        return new TakeCompleteTaskResponse(
            completion.TaskId,
            codeReview,
            skillGainAnalysis,
            assessmentId);
    }

    private async Task<SkillGainAnalysisResponse> AnalyzeSkillGainsAsync(
        TaskRecommendationContextDto task,
        WorkFit.TalentManagement.Contracts.Dtos.EmployeeDetailsDto employee,
        CodeReviewWorkflowExecutionResult codeReview,
        CancellationToken cancellationToken)
    {
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

        var employeeSkills = employee.Skills
            .Select(skill => new EmployeeSkillGainInput(
                skill.SkillId,
                skill.SkillName,
                Math.Clamp(skill.ConfidenceScore, 0, 100)))
            .ToArray();

        var review = codeReview.Response;
        var codeReviewInput = new CodeReviewGainInput(
            review.OverallScore,
            review.Risk,
            review.TechnicalDebt,
            codeReview.ExecutiveSummary,
            codeReview.DeveloperSummary,
            review.PositiveFindings,
            review.Issues
                .Select(issue => new CodeReviewIssueGainInput(
                    issue.Title,
                    issue.Severity,
                    issue.Detail,
                    issue.File))
                .ToArray(),
            review.Recommendations);

        return await _skillGainAnalysisService.AnalyzeAsync(
            new SkillGainAnalysisContext(
                task.Id,
                task.ProjectId,
                task.OrganizationId,
                task.Title,
                task.Description,
                task.ProjectName,
                task.ProjectDescription,
                requiredSkills,
                employee.Id,
                employee.Name,
                employee.JobTitle,
                employeeSkills,
                codeReviewInput),
            cancellationToken);
    }

    private async Task<Guid?> CreateTeamLeadAssessmentAsync(
        TaskRecommendationContextDto task,
        WorkFit.TalentManagement.Contracts.Dtos.EmployeeDetailsDto employee,
        SkillGainAnalysisResponse analysis,
        CancellationToken cancellationToken)
    {
        var skillChanges = new List<(Guid skillId, string skillName, int oldScore, int proposedScore, string evidenceDesc)>();

        foreach (var change in analysis.SkillChanges)
        {
            skillChanges.Add((change.SkillId, change.SkillName, change.OldScore, change.NewScore, change.Reasoning));
        }

        foreach (var newSkill in analysis.NewSkills)
        {
            var resolved = await _skillCatalog.ResolveOrCreateSkillAsync(newSkill.SkillName, cancellationToken);
            skillChanges.Add((resolved.SkillId, resolved.Name, 0, newSkill.NewScore, newSkill.Reasoning));
        }

        if (skillChanges.Count == 0 || !task.TeamLeaderId.HasValue)
        {
            if (skillChanges.Count == 0)
            {
                _logger.LogInformation(
                    "Skipping team-lead assessment for task {TaskId} because no skill changes were identified.",
                    task.Id);
            }

            return null;
        }

        return await _createAssessmentService.CreateAsync(
            employeeProfileId: employee.Id,
            employeeUserId: employee.UserId,
            description: $"Team-lead skill assessment generated after completing task '{task.Title}' with AI code review and skill-gain analysis.",
            type: AssessmentType.TeamLeadAssessment,
            skillChanges: skillChanges,
            taskId: task.Id,
            teamLeadId: task.TeamLeaderId.Value);
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
}