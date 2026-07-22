using Microsoft.Extensions.Logging;
using WorkFit.Integration.Contracts.IntegrationSyncService;
using WorkFit.Integration.Contracts.ProjectManagementProvider;
using WorkFit.Integration.Features.Shared;
using WorkFit.Integration.Infrastructure.Data;
using WorkFit.Integration.Infrastructure.Providers.Jira;
using WorkFit.ProjectManagement.Contracts.CreateProjectService;
using WorkFit.ProjectManagement.Contracts.CreateProjectTaskService;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.Skills.Contracts;
using WorkFit.Skills.Contracts.Dtos;
using WorkFit.TalentManagement.Contracts.WriteServices.CreateEmployee;
using WorkFit.TalentManagement.Contracts.WriteServices.CreateOrUpdateSkill;

namespace WorkFit.Integration.Features.Commands.SyncIntegration;

internal sealed class SyncIntegrationCommandHandler : IRequestHandler<SyncIntegrationCommand, SyncResult>
{
    private readonly IProjectManagementProvider _provider;
    private readonly ICreateProjectService _createProjectService;
    private readonly ICreateProjectTaskService _createTaskService;
    private readonly ISkillCatalog _skillCatalog;
    private readonly IGetOrCreateExternalEmployeeService _getOrCreateExternalEmployee;
    private readonly ICreateOrUpdateEmployeeSkillsAfterAssessmentService _updateSkills;
    private readonly ILogger<SyncIntegrationCommandHandler> _logger;

    private static readonly Guid SystemAssessmentId = new("00000000-0000-0000-0000-000000000001");

    public SyncIntegrationCommandHandler(
        IProjectManagementProvider provider,
        ICreateProjectService createProjectService,
        ICreateProjectTaskService createTaskService,
        ISkillCatalog skillCatalog,
        IGetOrCreateExternalEmployeeService getOrCreateExternalEmployee,
        ICreateOrUpdateEmployeeSkillsAfterAssessmentService updateSkills,
        ILogger<SyncIntegrationCommandHandler> logger)
    {
        _provider = provider;
        _createProjectService = createProjectService;
        _createTaskService = createTaskService;
        _skillCatalog = skillCatalog;
        _getOrCreateExternalEmployee = getOrCreateExternalEmployee;
        _updateSkills = updateSkills;
        _logger = logger;
    }

    public async Task<SyncResult> Handle(SyncIntegrationCommand request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Integration sync started. Provider={Provider}, Org={OrgId}",
            _provider.ProviderName, request.OrganizationId);

        await _provider.InitializeForOrganizationAsync(request.OrganizationId, cancellationToken);

        int projectsSynced = 0;
        int tasksSynced = 0;
        int developersSynced = 0;
        int skillsSynced = 0;
        var errors = new List<string>();

        // ── 1. Developers + skill signals ─────────────────────────────────────
        var developers = await _provider.FetchDeveloperProfilesAsync(cancellationToken);
        var accountIdToEmployeeId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        foreach (var dev in developers)
        {
            try
            {
                var empId = await SyncDeveloperAsync(dev, request.OrganizationId, cancellationToken);
                developersSynced++;
                accountIdToEmployeeId[dev.SourceAccountId] = empId;

                if (dev.SkillSignals.Count > 0)
                {
                    var synced = await SyncSkillSignalsAsync(empId, dev.SkillSignals, cancellationToken);
                    skillsSynced += synced;
                }
            }
            catch (Exception ex)
            {
                var msg = $"[Developer] AccountId={dev.SourceAccountId} ({dev.DisplayName}): {ex.Message}";
                _logger.LogWarning(ex, msg);
                errors.Add(msg);
            }
        }

        // ── 2. Projects ───────────────────────────────────────────────────────
        var externalProjects = await _provider.FetchProjectsAsync(cancellationToken);
        var projectKeyToId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        foreach (var ext in externalProjects)
        {
            try
            {
                var projectId = await UpsertProjectAsync(ext, request.OrganizationId, cancellationToken);
                projectKeyToId[ext.SourceKey] = projectId;
                projectsSynced++;
            }
            catch (Exception ex)
            {
                var msg = $"[Project] SourceKey={ext.SourceKey}: {ex.Message}";
                _logger.LogWarning(ex, msg);
                errors.Add(msg);
            }
        }

        // ── 3. Tasks ──────────────────────────────────────────────────────────
        foreach (var (projectKey, projectId) in projectKeyToId)
        {
            IReadOnlyList<ExternalTaskDto> externalTasks;
            try
            {
                externalTasks = await _provider.FetchTasksAsync(projectKey, cancellationToken);
            }
            catch (Exception ex)
            {
                var msg = $"[FetchTasks] ProjectKey={projectKey}: {ex.Message}";
                _logger.LogWarning(ex, msg);
                errors.Add(msg);
                continue;
            }

            foreach (var ext in externalTasks)
            {
                try
                {
                    await UpsertTaskAsync(ext, projectId, accountIdToEmployeeId, cancellationToken);
                    tasksSynced++;
                }
                catch (Exception ex)
                {
                    var msg = $"[Task] SourceKey={ext.SourceKey}: {ex.Message}";
                    _logger.LogWarning(ex, msg);
                    errors.Add(msg);
                }
            }
        }

        var result = new SyncResult(
            _provider.ProviderName,
            projectsSynced,
            tasksSynced,
            developersSynced,
            skillsSynced,
            errors.Count,
            errors.AsReadOnly(),
            DateTimeOffset.UtcNow);

        _logger.LogInformation(
            "Integration sync completed. Projects={P}, Tasks={T}, Developers={D}, Skills={S}, Errors={E}",
            projectsSynced, tasksSynced, developersSynced, skillsSynced, errors.Count);

        return result;
    }

    private async Task<Guid> UpsertProjectAsync(
        ExternalProjectDto ext,
        Guid organizationId,
        CancellationToken ct)
    {
        var dto = new UpsertExternalProjectDto(
            OrganizationId: organizationId,
            SourceSystem: _provider.ProviderName,
            SourceReferenceId: ext.SourceKey,
            Name: Truncate(ext.Name, 100)!,
            Description: Truncate(ext.Description, 500)
        );

        return await _createProjectService.UpsertExternalProjectAsync(dto, ct);
    }

    private async Task UpsertTaskAsync(
        ExternalTaskDto ext,
        Guid projectId,
        Dictionary<string, Guid> accountIdToEmployeeId,
        CancellationToken ct)
    {
        var taskType = JiraIssueMapper.MapTaskType(ext.IssueType);
        var priority = JiraIssueMapper.MapTaskPriority(ext.Priority);
        var status = JiraIssueMapper.MapTaskStatus(ext.Status);

        Guid? resolvedAssigneeId = null;
        if (!string.IsNullOrWhiteSpace(ext.AssigneeAccountId) && accountIdToEmployeeId.TryGetValue(ext.AssigneeAccountId, out var empId))
        {
            resolvedAssigneeId = empId;
        }

        var dto = new UpsertExternalTaskDto(
            ProjectId: projectId,
            SourceSystem: _provider.ProviderName,
            SourceReferenceId: ext.SourceKey,
            Title: Truncate(ext.Title, 100) ?? "(no title)",
            Description: Truncate(ext.Description, 500),
            TaskType: taskType,
            Priority: priority,
            Status: status,
            AssigneeId: resolvedAssigneeId,
            StoryPoints: ext.StoryPoints,
            CompletedAt: ext.ResolvedAt,
            UpdatedAt: ext.ResolvedAt,
            0
        );

        await _createTaskService.UpsertExternalTaskAsync(dto, ct);
    }

    private async Task<Guid> SyncDeveloperAsync(
        ExternalDeveloperDto dev,
        Guid organizationId,
        CancellationToken ct)
    {
        var deterministicUserId = DeterministicGuid(dev.SourceAccountId);

        var empId = await _getOrCreateExternalEmployee.GetOrCreateAsync(
            organizationId: organizationId,
            userId: deterministicUserId,
            sourceSystem: _provider.ProviderName,
            externalAccountId: dev.SourceAccountId,
            externalDisplayName: dev.DisplayName,
            email: dev.Email,
            jobTitle: dev.JobTitle ?? "Software Engineer",
            cancellationToken: ct);

        return empId;
    }

    private async Task<int> SyncSkillSignalsAsync(
        Guid employeeId,
        IReadOnlyList<ExternalSkillSignalDto> signals,
        CancellationToken ct)
    {
        var skillDetails = new List<SkillDetails>();

        foreach (var signal in signals)
        {
            try
            {
                var resolved = await _skillCatalog.ResolveOrCreateSkillAsync(signal.SkillName, ct);
                skillDetails.Add(new SkillDetails(
                    resolved.SkillId,
                    resolved.Name,
                    (int)signal.ConfidenceScore,
                    signal.Evidence,
                    signal.SourceLabel));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to resolve skill '{Skill}' for employee {EmpId}.",
                    signal.SkillName, employeeId);
            }
        }

        if (skillDetails.Count == 0) return 0;

        await _updateSkills.CreateOrUpdateAsync(employeeId, SystemAssessmentId, skillDetails, ct);
        return skillDetails.Count;
    }

    private static string? Truncate(string? s, int max) =>
        s is null ? null : s.Length <= max ? s : s[..max];

    private static Guid DeterministicGuid(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}

