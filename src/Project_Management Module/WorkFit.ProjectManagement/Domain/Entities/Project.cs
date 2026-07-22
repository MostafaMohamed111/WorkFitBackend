using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Domain.Exceptions;
using WorkFit.SharedKernel.BaseEntity;

namespace WorkFit.ProjectManagement.Domain.Entities;

public class Project : BaseEntity
{
    public Guid OrganizationId { get; private set; } // ref to organization
    private readonly List<Guid> _projectDocumentIds = new(); // ref to documents associated with the project
    public IReadOnlyCollection<Guid> ProjectDocumentIds => _projectDocumentIds;
    public string Name { get; private set; }
    public string? Description { get; private set; }

    public ProjectStatus Status { get; private set; }

    public DateOnly? StartDate { get; private set; }

    public DateOnly? EndDate { get; private set; }
    public Guid? TeamLeaderId { get; private set; }
    public SourceSystem? SourceSystem { get; private set; }

    public string? SourceReferenceId { get; private set; }

    public ICollection<ProjectRequiredSkill> RequiredSkills { get; private set; } = new List<ProjectRequiredSkill>();

    private readonly List<ProjectTask> _tasks = new ();
    public IReadOnlyCollection<ProjectTask> Tasks => _tasks;

    private readonly List<Guid> _assignedEmployees = new ();
    public IReadOnlyCollection<Guid> AssignedEmployees => _assignedEmployees;

    private readonly List<ProjectActivityLog> _activityLogs = new List<ProjectActivityLog>();
    public IReadOnlyCollection<ProjectActivityLog> ActivityLogs => _activityLogs;

    private Project() { } // EF

    public static Project Create(
        Guid organizationId,
        string name,
        List<Guid> projectDocumentIds,
        string? description,
        DateOnly? startDate,
        DateOnly? endDate,
        Guid? teamLeaderId,
        ProjectStatus status = ProjectStatus.Planning,
        SourceSystem sourceSystem = Enums.SourceSystem.Internal,
        string? sourceReferenceId = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3 || name.Length > 100)
            throw new ArgumentException("Project name must be between 3 and 100 characters.", nameof(name));

        if (description is not null && description.Length > 500)
            throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));

        if (startDate.HasValue && endDate.HasValue && endDate <= startDate)
            throw new ArgumentException("End date must be after start date.");
        if (sourceSystem == Enums.SourceSystem.Internal &&
                (teamLeaderId is null || teamLeaderId == Guid.Empty))
        {
            throw new ArgumentException(
                "Team leader is required for internal projects.",
                nameof(teamLeaderId));
        }

        var project = new Project
        {
            OrganizationId = organizationId,
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            TeamLeaderId = teamLeaderId,
            Status = status,
            SourceSystem = sourceSystem,
            SourceReferenceId = sourceReferenceId,
        };
        foreach (var docId in projectDocumentIds)
        {
            if (docId == Guid.Empty)
                throw new ArgumentException("Project document ID cannot be empty.", nameof(projectDocumentIds));
            project._projectDocumentIds.Add(docId);
        }

        var log = ProjectActivityLog.Create(
            project.Id,
            teamLeaderId,
            ActivityActions.ProjectCreated,
            ActivityEntityType.Project,
            project.Id);
        project._activityLogs.Add(log);
        return project;
    }

    public void UpdateDetails(string? name, string? description, DateOnly? endDate)
    {
        if (name is not null)
        {
            if (name.Length < 3 || name.Length > 100)
                throw new ArgumentException("Project name must be between 3 and 100 characters.", nameof(name));
            Name = name;
        }

        if (description is not null)
        {
            if (description.Length > 500)
                throw new ArgumentException("Description cannot exceed 500 characters.", nameof(description));
            Description = description;
        }

        if (endDate.HasValue)
        {
            if (StartDate.HasValue && endDate <= StartDate)
                throw new ArgumentException("End date must be after start date.");
            EndDate = endDate;
        }

        MarkUpdated();
    }

    public void ReplaceRequiredSkills(ICollection<ProjectRequiredSkill> requiredSkills)
    {
        RequiredSkills = requiredSkills;
        MarkUpdated();
    }

    public void ChangeStatus(Guid teamLeaderId, ProjectStatus newStatus)
    {
        if (!Status.CanTransitionToInternal(newStatus))
            throw new InvalidOperationException($"Cannot transition project from '{Status}' to '{newStatus}'.");
        var beforeStatus = Status;
        Status = newStatus;
        var log = ProjectActivityLog.Create(
        Id,
        teamLeaderId,
        ActivityActions.ProjectStatusChanged,
        ActivityEntityType.Project,
        Id,
        beforeState: $"{{\"status\":\"{beforeStatus.ToString()}\"}}",
        afterState: $"{{\"status\":\"{Status.ToString()}\"}}");

        _activityLogs.Add(log);
        MarkUpdated();
    }

    /// <summary>
    /// Soft-archives the project (DELETE /api/projects/{id}). Hard delete is never performed.
    /// </summary>
    public void Archive(Guid teamLead)
    {

        var log = ProjectActivityLog.Create(
            Id,
            teamLead,
            ActivityActions.ProjectArchived,
            ActivityEntityType.Project,
            Id);

        _activityLogs.Add(log);
        Status = ProjectStatus.Cancelled;
        MarkUpdated();
    }

    // creating a task is done through the project aggregate root to ensure that the task is always associated with a valid project.
    public ProjectTask CreateTask(Guid projectId,
       string title,
       string? description,
       TaskType taskType,
       TaskPriority priority,
       Guid createdById,
       Guid? assigneeId,
       int? storyPoints,
       DateOnly? dueDate,
       int allocationPercentage)
    {
        if (projectId != Id)
            throw new InvalidProjectForCreatingTaskDomainException(projectId);
        var task = ProjectTask.Create(
            projectId: projectId,
            title: title,
            description: description,
            taskType: taskType,
            priority: priority,
            createdById: createdById,
            assigneeId: assigneeId,
            storyPoints: storyPoints,
            dueDate: dueDate,
            allocationPercentage: allocationPercentage
        );

        if(Tasks.Any(t => t.Title == title))
            throw new TaskAlreadyCreatedForProjectDomainException(title);
        if (assigneeId.HasValue)
            if(!AssignedEmployees.Contains(assigneeId.Value))
                _assignedEmployees.Add(assigneeId.Value);
        
        _tasks.Add(task);
        return task;

    }

    public ProjectTask AssignEmployeeForTask(Guid taskId, Guid EmployeeId, int? allocationPercentage)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == taskId);
        if (task is null)
            throw new TaskNotFoundForProjectDomainException(taskId, Id);

        task.Assign(EmployeeId, allocationPercentage);
        return task;    
    }
}

file static class ProjectStatusInternalExtensions
{
    // Mirrors WorkFit.ProjectManagement.Features.Common.EnumMappingExtensions.CanTransitionTo,
    // kept internal to the domain layer so Project stays free of a Features-layer dependency.
    public static bool CanTransitionToInternal(this ProjectStatus current, ProjectStatus target)
    {
        if (current == target) return false;

        return current switch
        {
            ProjectStatus.Planning => target is ProjectStatus.Active or ProjectStatus.Cancelled,
            ProjectStatus.Active => target is ProjectStatus.OnHold or ProjectStatus.Completed or ProjectStatus.Cancelled,
            ProjectStatus.OnHold => target is ProjectStatus.Active or ProjectStatus.Cancelled,
            ProjectStatus.Completed => false,
            ProjectStatus.Cancelled => false,
            _ => false
        };
    }
}
