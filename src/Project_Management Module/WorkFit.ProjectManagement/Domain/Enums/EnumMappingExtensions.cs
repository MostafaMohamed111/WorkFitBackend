using WorkFit.ProjectManagement.Domain.Enums;

namespace WorkFit.ProjectManagement.Features.Common;

/// <summary>
/// Maps domain enums to/from the snake_case string values used on the wire
/// (e.g. ProjectStatus.OnHold <-> "on_hold"), matching Module 5 API contracts.
/// </summary>
public static class EnumMappingExtensions
{
    public static string ToApiString(this ProjectStatus status) => status switch
    {
        ProjectStatus.Planning => "planning",
        ProjectStatus.Active => "active",
        ProjectStatus.OnHold => "on_hold",
        ProjectStatus.Completed => "completed",
        ProjectStatus.Cancelled => "cancelled",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    public static ProjectStatus ToProjectStatus(this string value) => value?.Trim().ToLowerInvariant() switch
    {
        "planning" => ProjectStatus.Planning,
        "active" => ProjectStatus.Active,
        "on_hold" => ProjectStatus.OnHold,
        "completed" => ProjectStatus.Completed,
        "cancelled" => ProjectStatus.Cancelled,
        _ => throw new ArgumentException($"Unknown project status '{value}'.", nameof(value))
    };

    public static string ToApiString(this SkillLevel level) => level switch
    {
        SkillLevel.Beginner => "beginner",
        SkillLevel.Proficient => "proficient",
        SkillLevel.Expert => "expert",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
    };

    public static SkillLevel ToSkillLevel(this string value) => value?.Trim().ToLowerInvariant() switch
    {
        "beginner" => SkillLevel.Beginner,
        "proficient" => SkillLevel.Proficient,
        "expert" => SkillLevel.Expert,
        _ => throw new ArgumentException($"Unknown skill level '{value}'.", nameof(value))
    };

    /// <summary>
    /// Valid project status transitions for PUT /api/projects/{id}/status.
    /// </summary>
    public static bool CanTransitionTo(this ProjectStatus current, ProjectStatus target)
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
