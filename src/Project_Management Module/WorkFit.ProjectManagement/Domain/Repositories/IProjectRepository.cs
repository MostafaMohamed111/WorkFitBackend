using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Features.Project.GetProjectById;
using WorkFit.ProjectManagement.Features.Project.GetProjectDomains;
using WorkFit.ProjectManagement.Features.Project.GetProjects;
namespace WorkFit.ProjectManagement.Infrastructure.Data.Repositories;



public interface IProjectRepository
{
    Task<IReadOnlyList<ProjectListItemDto>> GetProjectsAsync(
        string? status,
        Guid? departmentId,
        int page,
        int limit,
        CancellationToken ct);

    //Task<ProjectDetailDto?> GetByIdAsync(
    //    Guid id,
    //    CancellationToken ct);

    Task AddAsync(Project project, CancellationToken ct);

    Task UpdateAsync(Project project, CancellationToken ct);

    Task ArchiveAsync(Guid id, CancellationToken ct);
    public Task<ProjectDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken ct);
    public Task<Project?> GetByIdAsync(Guid id, CancellationToken ct); 
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
    Task<bool> DomainTagExistsAsync(Guid projectId, Guid domainId, CancellationToken ct);
    public Task<IReadOnlyList<ProjectDomainDto>> GetDomainsAsync(Guid projectId, CancellationToken ct); 
    Task AddDomainAsync(ProjectDomain projectDomain, CancellationToken ct);

    Task<bool> RemoveDomainAsync(Guid projectId, Guid domainId, CancellationToken ct);
    Task AddActivityLogAsync(Domain.Entities.ProjectActivityLog log, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}