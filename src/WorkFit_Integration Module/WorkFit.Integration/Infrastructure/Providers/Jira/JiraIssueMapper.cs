namespace WorkFit.Integration.Infrastructure.Providers.Jira;

/// <summary>
/// Static mappers specifically kept public because they are referenced by
/// SyncIntegrationCommandHandler for mapping generic task concepts.
/// </summary>
public static class JiraIssueMapper
{
    public static string MapTaskStatus(string jiraStatus) =>
        jiraStatus.Trim().ToLowerInvariant() switch
        {
            "to do"       => "ToDo",
            "open"        => "ToDo",
            "backlog"     => "ToDo",
            "in progress" => "InProgress",
            "in review"   => "Review",
            "review"      => "Review",
            "code review" => "Review",
            "done"        => "Done",
            "closed"      => "Done",
            "resolved"    => "Done",
            _             => "ToDo"
        };

    public static string MapTaskPriority(string jiraPriority) =>
        jiraPriority.Trim().ToLowerInvariant() switch
        {
            "lowest"   => "Low",
            "low"      => "Low",
            "medium"   => "Medium",
            "high"     => "High",
            "highest"  => "Critical",
            "critical" => "Critical",
            _          => "Medium"
        };

    public static string MapTaskType(string jiraType) =>
        jiraType.Trim().ToLowerInvariant() switch
        {
            "story"    => "Story",
            "bug"      => "Bug",
            "epic"     => "Epic",
            "task"     => "Task",
            "sub-task" => "SubTask",
            "subtask"  => "SubTask",
            _          => "Task"
        };
}
