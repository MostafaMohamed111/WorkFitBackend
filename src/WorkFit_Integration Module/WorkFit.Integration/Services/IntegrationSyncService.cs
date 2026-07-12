using Microsoft.Extensions.Logging;
using WorkFit.Integration.Contracts.Abstractions;
using WorkFit.Integration.Contracts.Dtos;
using WorkFit.Integration.Providers.Jira.Mappers;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.Skills.Contracts;
using WorkFit.TalentManagement.Contracts.WriteServices.CreateEmployee;
using WorkFit.TalentManagement.Contracts.WriteServices.CreateOrUpdateSkill;

namespace WorkFit.Integration.Services;

/// <summary>
/// Orchestrates a full sync from an IProjectManagementProvider into the WorkFit database.
///
/// Flow:
///   1. Fetch projects from provider → upsert into ProjectManagement module.
///   2. Fetch tasks per project     → upsert into ProjectManagement module.
///   3. Fetch developer profiles    → upsert EmployeeProfiles + EmployeeSkills via module contracts.
///
/// All writes go through existing module contracts/repositories — this layer never
/// bypasses module boundaries.
/// </summary>
public sealed class IntegrationSyncService : IIntegrationSyncService
{
    private readonly IProjectManagementProvider _provider;
    private readonly WorkFitProjectDbContext _projectDb;
    private readonly ISkillCatalog _skillCatalog;
    private readonly IGetOrCreateExternalEmployeeService _getOrCreateExternalEmployee;
    private readonly ICreateOrUpdateEmployeeSkillsAfterAssessmentService _updateSkills;
    private readonly ILogger<IntegrationSyncService> _logger;

    // A synthetic Guid used as the "system" assessor for integration-sourced skills
    private static readonly Guid SystemAssessmentId = new("00000000-0000-0000-0000-000000000001");

    public IntegrationSyncService(
        IProjectManagementProvider provider,
        WorkFitProjectDbContext projectDb,
        ISkillCatalog skillCatalog,
        IGetOrCreateExternalEmployeeService getOrCreateExternalEmployee,
        ICreateOrUpdateEmployeeSkillsAfterAssessmentService updateSkills,
        ILogger<IntegrationSyncService> logger)
    {
        _provider      = provider;
        _projectDb     = projectDb;
        _skillCatalog  = skillCatalog;
        _getOrCreateExternalEmployee = getOrCreateExternalEmployee;
        _updateSkills  = updateSkills;
        _logger        = logger;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Main entry point
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<SyncResult> SyncAsync(
        Guid organizationId,
        Guid departmentId,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Integration sync started. Provider={Provider}, Org={OrgId}, Dept={DeptId}",
            _provider.ProviderName, organizationId, departmentId);

        // ── 0. Load organization-specific provider settings from the DB ───────
        await _provider.InitializeForOrganizationAsync(organizationId, ct);

        int projectsSynced  = 0;
        int tasksSynced     = 0;
        int developersSynced = 0;
        int skillsSynced    = 0;
        var errors          = new List<string>();

        // ── 1. Developers + skill signals ─────────────────────────────────────
        var developers = await _provider.FetchDeveloperProfilesAsync(ct);
        var accountIdToEmployeeId = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        foreach (var dev in developers)
        {
            try
            {
                var empId = await SyncDeveloperAsync(dev, organizationId, ct);
                developersSynced++;
                accountIdToEmployeeId[dev.SourceAccountId] = empId;

                if (dev.SkillSignals.Count > 0)
                {
                    var synced = await SyncSkillSignalsAsync(empId, dev.SkillSignals, ct);
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
        var externalProjects = await _provider.FetchProjectsAsync(ct);
        var projectKeyToId   = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        foreach (var ext in externalProjects)
        {
            try
            {
                var projectId = await UpsertProjectAsync(ext, departmentId, ct);
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

        // Flush projects to the DB before syncing tasks to avoid FK constraint violations
        try
        {
            await _projectDb.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            var msg = $"[SaveChanges:Projects] {ex.Message}";
            _logger.LogError(ex, msg);
            errors.Add(msg);
            // If we can't save projects, we shouldn't attempt tasks
            return new SyncResult(_provider.ProviderName, projectsSynced, tasksSynced, developersSynced, skillsSynced, errors.Count, errors.AsReadOnly(), DateTimeOffset.UtcNow);
        }

        // ── 3. Tasks ──────────────────────────────────────────────────────────
        foreach (var (projectKey, projectId) in projectKeyToId)
        {
            IReadOnlyList<ExternalTaskDto> externalTasks;
            try
            {
                externalTasks = await _provider.FetchTasksAsync(projectKey, ct);
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
                    await UpsertTaskAsync(ext, projectId, accountIdToEmployeeId, ct);
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

        // Save project + task changes
        try
        {
            await _projectDb.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            var msg = $"[SaveChanges:Projects] {ex.Message}";
            _logger.LogError(ex, msg);
            errors.Add(msg);
        }

        var result = new SyncResult(
            ProviderName:      _provider.ProviderName,
            ProjectsSynced:    projectsSynced,
            TasksSynced:       tasksSynced,
            DevelopersSynced:  developersSynced,
            SkillSignalsSynced: skillsSynced,
            Errors:            errors.Count,
            ErrorMessages:     errors.AsReadOnly(),
            SyncedAt:          DateTimeOffset.UtcNow);

        _logger.LogInformation(
            "Integration sync completed. Projects={P}, Tasks={T}, Developers={D}, Skills={S}, Errors={E}",
            projectsSynced, tasksSynced, developersSynced, skillsSynced, errors.Count);

        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Project upsert
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<Guid> UpsertProjectAsync(
        ExternalProjectDto ext,
        Guid departmentId,
        CancellationToken ct)
    {
        // Check if already synced (match by SourceSystem + SourceReferenceId)
        var existing = _projectDb.Projects
            .FirstOrDefault(p =>
                p.SourceSystem == _provider.ProviderName &&
                p.SourceReferenceId == ext.SourceKey);

        if (existing is not null)
        {
            _logger.LogDebug("Project '{Key}' already exists (Id={Id}), skipping.", ext.SourceKey, existing.Id);
            return existing.Id;
        }

        var status = MapProjectStatus(ext.Status);
        var project = Project.Create(
            departmentId:      departmentId,
            name:              Truncate(ext.Name, 100),
            description:       Truncate(ext.Description, 500),
            startDate:         ext.StartDate,
            endDate:           ext.EndDate,
            status:            status,
            sourceSystem:      _provider.ProviderName,
            sourceReferenceId: ext.SourceKey);

        await _projectDb.Projects.AddAsync(project, ct);
        _logger.LogDebug("Created project '{Name}' from {Provider}.", ext.Name, _provider.ProviderName);
        return project.Id;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Task upsert
    // ─────────────────────────────────────────────────────────────────────────

    private async Task UpsertTaskAsync(
        ExternalTaskDto ext,
        Guid projectId,
        Dictionary<string, Guid> accountIdToEmployeeId,
        CancellationToken ct)
    {
        // Idempotency: skip if already present by source reference
        bool exists = _projectDb.ProjectTasks
            .Any(t =>
                t.SourceSystem == _provider.ProviderName &&
                t.SourceReferenceId == ext.SourceKey);

        if (exists)
        {
            _logger.LogDebug("Task '{Key}' already exists, skipping.", ext.SourceKey);
            return;
        }

        var taskType  = ParseEnum<TaskType>(JiraIssueMapper.MapTaskType(ext.IssueType),  TaskType.Task);
        var priority  = ParseEnum<TaskPriority>(JiraIssueMapper.MapTaskPriority(ext.Priority), TaskPriority.Medium);

        DateOnly? dueDate = ext.DueDate;

        var createdById = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(ext.ReporterAccountId) && accountIdToEmployeeId.TryGetValue(ext.ReporterAccountId, out var reporterEmpId))
        {
            createdById = reporterEmpId;
        }

        Guid? resolvedAssigneeId = null;
        if (!string.IsNullOrWhiteSpace(ext.AssigneeAccountId) && accountIdToEmployeeId.TryGetValue(ext.AssigneeAccountId, out var empId))
        {
            resolvedAssigneeId = empId;
        }

        var task = ProjectTask.Create(
            projectId:    projectId,
            title:        Truncate(ext.Title, 100) ?? "(no title)",
            description:  Truncate(ext.Description, 500),
            taskType:     taskType,
            priority:     priority,
            createdById:  createdById,
            assigneeId:   resolvedAssigneeId,
            storyPoints:  ext.StoryPoints,
            dueDate:      dueDate);

        // Wire source tracking via domain method (no reflection needed)
        task.SetSource(_provider.ProviderName, ext.SourceKey);

        // Apply the correct status (override the default ToDo)
        ApplyTaskStatus(task, JiraIssueMapper.MapTaskStatus(ext.Status));

        // Use reflection to forcefully inject historical Jira timestamps without modifying BaseEntity
        SetHistoricalTimestamps(task, ext.CreatedAt, ext.ResolvedAt);

        await _projectDb.ProjectTasks.AddAsync(task, ct);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Developer sync
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<Guid> SyncDeveloperAsync(
        ExternalDeveloperDto dev,
        Guid organizationId,
        CancellationToken ct)
    {
        // We pass a deterministic userId derived from the provider account id
        // so the same developer is never double-created.
        var deterministicUserId = DeterministicGuid(dev.SourceAccountId);

        var empId = await _getOrCreateExternalEmployee.GetOrCreateAsync(
            organizationId:      organizationId,
            userId:              deterministicUserId,
            sourceSystem:        _provider.ProviderName,
            externalAccountId:   dev.SourceAccountId,
            externalDisplayName: dev.DisplayName,
            email:               dev.Email,
            jobTitle:            dev.JobTitle ?? "Software Engineer",
            cancellationToken:   ct);

        _logger.LogDebug(
            "Synced developer '{Name}' → EmployeeId={EmpId}", dev.DisplayName, empId);

        return empId;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Skill signal sync
    // ─────────────────────────────────────────────────────────────────────────

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
                    skillId:    resolved.SkillId,
                    skillName:  resolved.Name,
                    skillScore: signal.ConfidenceScore,
                    details:    signal.Evidence,
                    source:     signal.SourceLabel));
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

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static ProjectStatus MapProjectStatus(string? status) =>
        status?.Trim().ToLowerInvariant() switch
        {
            "active"    => ProjectStatus.Active,
            "completed" => ProjectStatus.Completed,
            "cancelled" => ProjectStatus.Cancelled,
            "on_hold"   => ProjectStatus.OnHold,
            _           => ProjectStatus.Active
        };

    private static void ApplyTaskStatus(ProjectTask task, string statusName)
    {
        if (!Enum.TryParse<WorkFit.ProjectManagement.Domain.Enums.TaskStatus>(statusName, out var status))
            return;

        if (status == WorkFit.ProjectManagement.Domain.Enums.TaskStatus.Done)
        {
            try { task.Complete(); } catch { /* already done guard */ }
        }
        else if (status != WorkFit.ProjectManagement.Domain.Enums.TaskStatus.ToDo)
        {
            try { task.ChangeStatus(status); } catch { /* ignore invalid transitions */ }
        }
    }

    private static void SetHistoricalTimestamps(ProjectTask task, DateTimeOffset? createdAt, DateTimeOffset? resolvedAt)
    {
        var type = typeof(ProjectTask);
        var baseType = type.BaseType; // BaseEntity

        if (createdAt.HasValue)
        {
            var createdProp = baseType?.GetProperty("CreatedAt");
            createdProp?.SetValue(task, createdAt.Value.UtcDateTime);
        }

        if (resolvedAt.HasValue)
        {
            var updatedProp = baseType?.GetProperty("UpdatedAt");
            updatedProp?.SetValue(task, resolvedAt.Value.UtcDateTime);
            
            var completedProp = type.GetProperty("CompletedAt");
            completedProp?.SetValue(task, resolvedAt.Value);
        }
    }

    private static TEnum ParseEnum<TEnum>(string name, TEnum fallback)
        where TEnum : struct, Enum =>
        Enum.TryParse<TEnum>(name, out var val) ? val : fallback;

    private static string? Truncate(string? s, int max) =>
        s is null ? null : s.Length <= max ? s : s[..max];

    /// <summary>
    /// Generates a deterministic Guid from a string (v5 UUID using MD5 for simplicity).
    /// Ensures the same Jira accountId always maps to the same WorkFit userId.
    /// </summary>
    private static Guid DeterministicGuid(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
