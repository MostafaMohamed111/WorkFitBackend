namespace WorkFit.ProjectManagement.Contracts.CreateProjectTaskService;

public interface ICreateProjectTaskService
{
    Task<Guid> UpsertExternalTaskAsync(UpsertExternalTaskDto dto, CancellationToken cancellationToken = default);
}
