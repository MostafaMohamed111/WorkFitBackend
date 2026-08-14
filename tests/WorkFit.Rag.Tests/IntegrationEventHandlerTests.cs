using Microsoft.Extensions.Logging.Abstractions;
using WorkFit.ProjectManagement.Contracts.IntegrationEvents;
using WorkFit.ProjectManagement.Contracts.LookUpServices.TaskLookUp;
using WorkFit.Rag.Contracts.Indexing;
using WorkFit.Rag.CrossCutting;
using WorkFit.TalentManagement.Contracts.Indexing;
using WorkFit.TalentManagement.Contracts.IntegrationEvents;

namespace WorkFit.Rag.Tests;

public class IntegrationEventHandlerTests
{
    [Fact]
    public async Task ProjectTaskHandler_DeletedTask_DeletesIndexPoint()
    {
        var indexing = new FakeProjectTaskIndexingService();
        var handler = new ProjectTaskStateChangedIntegrationEventHandler(
            indexing, NullLogger<ProjectTaskStateChangedIntegrationEventHandler>.Instance);

        await handler.Handle(ProjectTaskEvent(isDeleted: true));

        Assert.True(indexing.Deleted);
        Assert.False(indexing.Upserted);
    }

    [Fact]
    public async Task ProjectTaskHandler_OpenTask_IndexesWithoutOutcomes()
    {
        var indexing = new FakeProjectTaskIndexingService();
        var handler = new ProjectTaskStateChangedIntegrationEventHandler(
            indexing, NullLogger<ProjectTaskStateChangedIntegrationEventHandler>.Instance);

        await handler.Handle(ProjectTaskEvent(status: "InProgress", assignedEmployeeId: Guid.NewGuid()));

        Assert.True(indexing.Upserted);
        Assert.NotNull(indexing.Document);
        Assert.Empty(indexing.Document!.EmployeeOutcomes);
    }

    [Fact]
    public async Task ProjectTaskHandler_CompletedAssignedTask_IndexesOutcome()
    {
        var indexing = new FakeProjectTaskIndexingService();
        var handler = new ProjectTaskStateChangedIntegrationEventHandler(
            indexing, NullLogger<ProjectTaskStateChangedIntegrationEventHandler>.Instance);
        var employeeId = Guid.NewGuid();

        await handler.Handle(ProjectTaskEvent(status: "Done", assignedEmployeeId: employeeId, storyPoints: 20));

        var outcome = Assert.Single(indexing.Document!.EmployeeOutcomes);
        Assert.Equal(employeeId, outcome.EmployeeProfileId);
        Assert.Equal(1.0, outcome.PerformanceScore, precision: 10);
    }

    [Fact]
    public async Task ProjectTaskHandler_CompletedWithoutAssignee_SkipsOutcome()
    {
        var indexing = new FakeProjectTaskIndexingService();
        var handler = new ProjectTaskStateChangedIntegrationEventHandler(
            indexing, NullLogger<ProjectTaskStateChangedIntegrationEventHandler>.Instance);

        await handler.Handle(ProjectTaskEvent(status: "Done", assignedEmployeeId: null, storyPoints: 10));

        Assert.True(indexing.Upserted);
        Assert.Empty(indexing.Document!.EmployeeOutcomes);
    }

    [Fact]
    public async Task EmployeeHandler_ActiveEmployee_UpsertsIndexPoint()
    {
        var indexing = new FakeEmployeeProfileIndexingService();
        var handler = new EmployeeIndexingStateChangedIntegrationEventHandler(
            indexing, NullLogger<EmployeeIndexingStateChangedIntegrationEventHandler>.Instance);

        await handler.Handle(EmployeeEvent("Active", "Created"));

        Assert.True(indexing.Upserted);
        Assert.Equal("Active", indexing.Document!.Status);
    }

    [Fact]
    public async Task EmployeeHandler_InactiveEmployee_DeletesIndexPoint()
    {
        var indexing = new FakeEmployeeProfileIndexingService();
        var handler = new EmployeeIndexingStateChangedIntegrationEventHandler(
            indexing, NullLogger<EmployeeIndexingStateChangedIntegrationEventHandler>.Instance);

        await handler.Handle(EmployeeEvent("Inactive", "Deactivated"));

        Assert.True(indexing.Deleted);
        Assert.False(indexing.Upserted);
    }

    private static ProjectTaskStateChangedIntegrationEvent ProjectTaskEvent(
        bool isDeleted = false,
        string status = "InProgress",
        Guid? assignedEmployeeId = null,
        int? storyPoints = null) => new(
        TaskId: Guid.NewGuid(),
        ProjectId: Guid.NewGuid(),
        OrganizationId: Guid.NewGuid(),
        TeamLeaderId: Guid.NewGuid(),
        ProjectName: "HCM Suite",
        ProjectDescription: null,
        ProjectStatus: "Active",
        ProjectStartDate: null,
        ProjectEndDate: null,
        ProjectSourceSystem: null,
        ProjectSourceReferenceId: null,
        GitHubRepositoryId: null,
        GitHubRepositoryName: null,
        Title: "Build payroll",
        Description: "Implement payroll.",
        TaskType: "Feature",
        Status: status,
        Priority: "High",
        StoryPoints: storyPoints,
        DueDate: null,
        AllocationPercentage: 20,
        AssignedEmployeeId: assignedEmployeeId,
        CreatedById: Guid.NewGuid(),
        ProjectRequiredSkills: new[]
        {
            new ProjectRequiredSkillContextDto(Guid.NewGuid(), "Proficient", 1)
        },
        SourceSystem: null,
        SourceReferenceId: null,
        GitHubBranchName: null,
        GitHubBranchNodeId: null,
        CreatedAt: DateTime.UtcNow,
        UpdatedAt: null,
        CompletedAt: status.Equals("Done", StringComparison.OrdinalIgnoreCase) ? DateTimeOffset.UtcNow : null,
        DeletedAt: null,
        IsDeleted: isDeleted,
        IsActive: true,
        Revision: 1,
        ChangeType: isDeleted ? "Deleted" : "Created",
        OccurredAt: DateTimeOffset.UtcNow);

    private static EmployeeIndexingStateChangedIntegrationEvent EmployeeEvent(string status, string changeType) => new(
        new EmployeeIndexingSnapshot(
            EmployeeProfileId: Guid.NewGuid(),
            OrganizationId: Guid.NewGuid(),
            Name: "Jane Doe",
            JobTitle: "Engineer",
            Bio: null,
            Status: status,
            CurrentAllocationPercentage: 0,
            HireDate: null,
            Skills: Array.Empty<EmployeeSkillIndexingSnapshot>(),
            Certifications: Array.Empty<EmployeeCertificationIndexingSnapshot>(),
            TaskPerformance: null,
            SnapshotAt: DateTimeOffset.UtcNow),
        ChangeType: changeType,
        OccurredAt: DateTimeOffset.UtcNow);
}

internal sealed class FakeProjectTaskIndexingService : IProjectTaskIndexingService
{
    public bool Upserted { get; private set; }
    public bool Deleted { get; private set; }
    public ProjectTaskIndexDocument? Document { get; private set; }

    public Task UpsertAsync(ProjectTaskIndexDocument document, CancellationToken cancellationToken = default)
    {
        Upserted = true;
        Document = document;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        Deleted = true;
        return Task.CompletedTask;
    }
}

internal sealed class FakeEmployeeProfileIndexingService : IEmployeeProfileIndexingService
{
    public bool Upserted { get; private set; }
    public bool Deleted { get; private set; }
    public EmployeeProfileIndexDocument? Document { get; private set; }

    public Task UpsertAsync(EmployeeProfileIndexDocument document, CancellationToken cancellationToken = default)
    {
        Upserted = true;
        Document = document;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid employeeProfileId, CancellationToken cancellationToken = default)
    {
        Deleted = true;
        return Task.CompletedTask;
    }
}
