using System;
using System.Collections.Generic;
using System.Text;

namespace WorkFit.ProjectManagement.Domain.Enums;

public static class ActivityActions
{
    public const string ProjectCreated = "project.created";
    public const string ProjectUpdated = "project.updated";
    public const string ProjectStatusChanged = "project.status_changed";
    public const string ProjectArchived = "project.archived";

    public const string TaskCreated = "task.created";
    public const string TaskUpdated = "task.updated";
    public const string TaskAssigned = "task.assigned";
    public const string TaskCompleted = "task.completed";
    public const string TaskDeleted = "task.deleted";

    public const string MemberAdded = "member.added";
    public const string MemberUpdated = "member.updated";
    public const string MemberRemoved = "member.removed";

    public const string ProjectDomainAdded = "project.domain_added";
    public const string ProjectDomainRemoved = "project.domain_removed";
}