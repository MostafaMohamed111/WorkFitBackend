using Microsoft.Extensions.Options;
using WorkFit.Engine.Contracts.AI;
using WorkFit.Rag.Contracts.Indexing;
using WorkFit.Rag.Infrastructure.Options;
using WorkFit.Rag.Infrastructure.Qdrant;

namespace WorkFit.Rag.Infrastructure.Indexing;

internal sealed class EmployeeProfileIndexingService(
    IEmbeddingClient embeddingClient,
    IQdrantRestClient qdrant,
    IOptions<QdrantOptions> qdrantOptions,
    IOptions<RagRecommendationOptions> recommendationOptions) : IEmployeeProfileIndexingService
{
    public async Task UpsertAsync(EmployeeProfileIndexDocument document, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        if (document.EmployeeProfileId == Guid.Empty || document.OrganizationId == Guid.Empty)
        {
            throw new ArgumentException("Employee profile and organization IDs are required.", nameof(document));
        }

        var text = $"Employee: {document.EmployeeName}\nStatus: {document.Status}\n" +
            $"Profile: {document.ProfileSummary}\nSkills: " +
            string.Join(", ", document.Skills.Select(skill => $"{skill.Name} ({skill.Level:0.##})"));
        var vector = await EmbedAsync(text, cancellationToken);
        var payload = new
        {
            employeeProfileId = document.EmployeeProfileId,
            organizationId = document.OrganizationId,
            employeeName = document.EmployeeName,
            status = document.Status,
            availableAllocation = Math.Max(0, document.AvailableAllocation),
            performanceScore = Normalize(document.PerformanceScore),
            skills = document.Skills.Select(skill => new
            {
                skillId = skill.SkillId,
                name = skill.Name,
                level = Math.Max(0, skill.Level)
            }).ToArray()
        };

        await qdrant.UpsertAsync(
            qdrantOptions.Value.EmployeeProfilesCollection,
            new QdrantPoint(DeterministicPointId.EmployeeProfile(document.EmployeeProfileId), vector, payload),
            cancellationToken);
    }

    public Task DeleteAsync(Guid employeeProfileId, CancellationToken cancellationToken = default)
    {
        if (employeeProfileId == Guid.Empty)
        {
            throw new ArgumentException("Employee profile ID is required.", nameof(employeeProfileId));
        }

        return qdrant.DeleteAsync(
            qdrantOptions.Value.EmployeeProfilesCollection,
            DeterministicPointId.EmployeeProfile(employeeProfileId),
            cancellationToken);
    }

    private async Task<ReadOnlyMemory<float>> EmbedAsync(string text, CancellationToken cancellationToken)
    {
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

        return response.Vectors[0];
    }

    private static double Normalize(double score) => double.IsFinite(score) ? Math.Clamp(score, 0, 1) : 0;
}
