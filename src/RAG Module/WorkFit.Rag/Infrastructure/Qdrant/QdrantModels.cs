using System.Text.Json;

namespace WorkFit.Rag.Infrastructure.Qdrant;

internal sealed record QdrantPoint(Guid Id, ReadOnlyMemory<float> Vector, object Payload);

internal sealed record QdrantSearchResult(Guid Id, double Score, JsonElement Payload);

internal sealed record QdrantFilter(IReadOnlyList<QdrantCondition> Must);

internal sealed record QdrantCondition(string Key, object? Match = null, object? Range = null);

internal sealed record IndexedEmployeeSkill(Guid? SkillId, string Name, double Level);

internal sealed record IndexedEmployeeProfile(
    Guid EmployeeProfileId,
    Guid OrganizationId,
    string EmployeeName,
    string Status,
    double AvailableAllocation,
    double PerformanceScore,
    IReadOnlyList<IndexedEmployeeSkill>? Skills);

internal sealed record IndexedProjectTask(
    Guid TaskId,
    Guid ProjectId,
    Guid OrganizationId,
    IReadOnlyList<IndexedEmployeeOutcome>? EmployeeOutcomes);

internal sealed record IndexedEmployeeOutcome(Guid EmployeeProfileId, double PerformanceScore);
