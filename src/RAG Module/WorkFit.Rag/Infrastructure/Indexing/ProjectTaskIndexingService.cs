using Microsoft.Extensions.Options;
using WorkFit.Engine.Contracts.AI;
using WorkFit.Rag.Contracts.Indexing;
using WorkFit.Rag.Infrastructure.Options;
using WorkFit.Rag.Infrastructure.Qdrant;

namespace WorkFit.Rag.Infrastructure.Indexing;

internal sealed class ProjectTaskIndexingService(
    IEmbeddingClient embeddingClient,
    IQdrantRestClient qdrant,
    IOptions<QdrantOptions> qdrantOptions,
    IOptions<RagRecommendationOptions> recommendationOptions) : IProjectTaskIndexingService
{
    public async Task UpsertAsync(ProjectTaskIndexDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.TaskId == Guid.Empty || document.ProjectId == Guid.Empty || document.OrganizationId == Guid.Empty)
        {
            throw new ArgumentException("Task, project, and organization IDs are required.", nameof(document));
        }

        var text = $"Project: {document.ProjectName}\nProject description: {document.ProjectDescription}\n" +
            $"Project status: {document.ProjectStatus}\nTask: {document.TaskTitle}\n" +
            $"Task description: {document.TaskDescription}\nTask type: {document.TaskType}\n" +
            $"Task status: {document.Status}\nPriority: {document.Priority}\n" +
            $"Story points: {document.StoryPoints}\nDue date: {document.DueDate}\n" +
            $"Required skills: {string.Join(", ", document.RequiredSkills)}";
        var response = await embeddingClient.EmbedAsync(
            new EmbeddingRequest(
                recommendationOptions.Value.EmbeddingModel,
                [text],
                EmbeddingTaskType.RetrievalDocument,
                QdrantOptions.VectorSize),
            cancellationToken);
        if (response.Vectors.Count != 1)
        {
            throw new InvalidOperationException("Embedding provider returned an unexpected vector count.");
        }

        var payload = new
        {
            taskId = document.TaskId,
            projectId = document.ProjectId,
            organizationId = document.OrganizationId,
            taskTitle = document.TaskTitle,
            taskType = document.TaskType,
            status = document.Status,
            priority = document.Priority,
            storyPoints = document.StoryPoints,
            projectName = document.ProjectName,
            projectStatus = document.ProjectStatus,
            requiredSkills = document.RequiredSkills,
            isActive = document.IsActive,
            revision = document.Revision,
            occurredAt = document.OccurredAt,
            employeeOutcomes = document.EmployeeOutcomes.Select(outcome => new
            {
                employeeProfileId = outcome.EmployeeProfileId,
                performanceScore = Normalize(outcome.PerformanceScore)
            }).ToArray()
        };
        await qdrant.UpsertAsync(
            qdrantOptions.Value.ProjectTasksCollection,
            new QdrantPoint(DeterministicPointId.ProjectTask(document.TaskId), response.Vectors[0], payload),
            cancellationToken);
    }

    public Task DeleteAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        if (taskId == Guid.Empty)
        {
            throw new ArgumentException("Task ID is required.", nameof(taskId));
        }

        return qdrant.DeleteAsync(
            qdrantOptions.Value.ProjectTasksCollection,
            DeterministicPointId.ProjectTask(taskId),
            cancellationToken);
    }

    private static double Normalize(double score) => double.IsFinite(score) ? Math.Clamp(score, 0, 1) : 0;
}
