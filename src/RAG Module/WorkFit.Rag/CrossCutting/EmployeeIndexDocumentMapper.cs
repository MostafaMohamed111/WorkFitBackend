using WorkFit.Rag.Contracts.Indexing;
using WorkFit.TalentManagement.Contracts.Indexing;

namespace WorkFit.Rag.CrossCutting;

internal static class EmployeeIndexDocumentMapper
{
    public static EmployeeProfileIndexDocument Map(EmployeeIndexingSnapshot employee)
    {
        ArgumentNullException.ThrowIfNull(employee);

        var skills = employee.Skills
            .Where(skill => !string.IsNullOrWhiteSpace(skill.Name))
            .Select(skill => new EmployeeSkillIndexDocument(
                skill.SkillId,
                IndexingSnapshotSanitizer.Required(skill.Name, "Unnamed skill"),
                Math.Clamp(skill.ConfidenceScore / 100d, 0, 1)))
            .ToArray();

        return new EmployeeProfileIndexDocument(
            employee.EmployeeProfileId,
            employee.OrganizationId,
            IndexingSnapshotSanitizer.Required(employee.Name, "Unnamed employee"),
            IndexingSnapshotSanitizer.Required(employee.Status, "Unknown"),
            Math.Max(0, 100 - employee.CurrentAllocationPercentage),
            PerformanceScore(employee.TaskPerformance),
            BuildProfileSummary(employee),
            skills);
    }

    public static bool IsRemoved(EmployeeIndexingSnapshot employee, string? changeType = null) =>
        employee.Status.Equals("Inactive", StringComparison.OrdinalIgnoreCase) ||
        employee.Status.Equals("Deleted", StringComparison.OrdinalIgnoreCase) ||
        changeType?.Equals("Deactivated", StringComparison.OrdinalIgnoreCase) == true ||
        changeType?.Equals("Deleted", StringComparison.OrdinalIgnoreCase) == true;

    private static double PerformanceScore(EmployeeTaskPerformanceIndexingSnapshot? performance)
    {
        if (performance is null || performance.AssignedTaskCount <= 0 || performance.CompletedTaskCount <= 0)
            return 0;

        var completionRatio = Math.Clamp(
            (double)performance.CompletedTaskCount / performance.AssignedTaskCount, 0, 1);
        var storyPointEvidence = Math.Clamp(performance.CompletedStoryPoints / 20d, 0, 1);
        return completionRatio * (0.5 + 0.5 * storyPointEvidence);
    }

    private static string BuildProfileSummary(EmployeeIndexingSnapshot employee)
    {
        var parts = new List<string>
        {
            $"Job title: {IndexingSnapshotSanitizer.Required(employee.JobTitle, "Not provided")}."
        };

        var bio = IndexingSnapshotSanitizer.Optional(employee.Bio);
        if (bio is not null)
            parts.Add($"Bio: {bio}.");

        var certifications = employee.Certifications
            .Where(certification => !certification.IsExpired)
            .Select(certification =>
                $"{IndexingSnapshotSanitizer.Required(certification.Name, "Unnamed certification")} " +
                $"from {IndexingSnapshotSanitizer.Required(certification.IssuingOrganization, "unknown issuer")}")
            .ToArray();
        if (certifications.Length > 0)
            parts.Add($"Certifications: {string.Join("; ", certifications)}.");

        var evidence = employee.Skills
            .SelectMany(skill => skill.Evidence.Select(item =>
                $"{IndexingSnapshotSanitizer.Required(skill.Name, "Skill")}: " +
                $"{IndexingSnapshotSanitizer.Required(item.Source, "Evidence")}: " +
                IndexingSnapshotSanitizer.Required(item.Details, "No details")))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToArray();
        if (evidence.Length > 0)
            parts.Add($"Skill evidence: {string.Join("; ", evidence)}.");

        return string.Join(" ", parts);
    }
}
