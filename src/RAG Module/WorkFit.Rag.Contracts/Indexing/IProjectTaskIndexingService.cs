namespace WorkFit.Rag.Contracts.Indexing;

public interface IProjectTaskIndexingService
{
    Task UpsertAsync(ProjectTaskIndexDocument document, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid taskId, CancellationToken cancellationToken = default);
}
