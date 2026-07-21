namespace WorkFit.ProjectManagement.Contracts.CreateProjectService;

public interface ICreateProjectService
{
    Task<Guid> UpsertExternalProjectAsync(UpsertExternalProjectDto dto, CancellationToken cancellationToken = default);
}
