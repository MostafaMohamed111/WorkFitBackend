using Microsoft.Extensions.Logging;
using WorkFit.ProjectManagement.Contracts.IntegrationEvents;
using WorkFit.Rag.Contracts.Indexing;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Rag.CrossCutting;

internal sealed class ProjectTaskStateChangedIntegrationEventHandler(
    IProjectTaskIndexingService indexingService,
    ILogger<ProjectTaskStateChangedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<ProjectTaskStateChangedIntegrationEvent>
{
    public async Task Handle(
        ProjectTaskStateChangedIntegrationEvent @event,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        try
        {
            if (@event.IsDeleted)
            {
                await indexingService.DeleteAsync(@event.TaskId, cancellationToken);
                return;
            }

            var requirements = @event.ProjectRequiredSkills
                .Select(skill => $"{skill.SkillId}: " +
                    $"{IndexingSnapshotSanitizer.Required(skill.Level, "Unspecified")} (priority {Math.Max(0, skill.Priority)})")
                .ToArray();
            var completed = @event.CompletedAt.HasValue ||
                @event.Status.Equals("Done", StringComparison.OrdinalIgnoreCase) ||
                @event.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
            var outcomes = completed && @event.AssignedEmployeeId.HasValue
                ? new[]
                {
                    new EmployeeTaskOutcomeIndexDocument(
                        @event.AssignedEmployeeId.Value,
                        CompletedOutcomeScore(@event.StoryPoints))
                }
                : [];

            var document = new ProjectTaskIndexDocument(
                @event.TaskId,
                @event.ProjectId,
                @event.OrganizationId,
                IndexingSnapshotSanitizer.Required(@event.Title, "Untitled task"),
                IndexingSnapshotSanitizer.Optional(@event.Description),
                IndexingSnapshotSanitizer.Required(@event.TaskType, "Unspecified"),
                IndexingSnapshotSanitizer.Required(@event.Status, "Unknown"),
                IndexingSnapshotSanitizer.Required(@event.Priority, "Unspecified"),
                @event.StoryPoints is >= 0 ? @event.StoryPoints : null,
                @event.DueDate,
                IndexingSnapshotSanitizer.Required(@event.ProjectName, "Unnamed project"),
                IndexingSnapshotSanitizer.Optional(@event.ProjectDescription),
                IndexingSnapshotSanitizer.Required(@event.ProjectStatus, "Unknown"),
                requirements,
                outcomes,
                @event.IsActive,
                Math.Max(0, @event.Revision),
                @event.OccurredAt);

            await indexingService.UpsertAsync(document, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Project task RAG indexing failed for task {TaskId} at revision {Revision}.",
                @event.TaskId,
                @event.Revision);
        }
    }

    private static double CompletedOutcomeScore(int? storyPoints)
    {
        var storyPointEvidence = Math.Clamp(Math.Max(0, storyPoints ?? 0) / 20d, 0, 1);
        return 0.5 + 0.5 * storyPointEvidence;
    }
}
