using System;
using System.Collections.Generic;
using System.Text;
using WorkFit.ProjectManagement.Domain.Entities;
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

    Task<bool> ExistsAsync(Guid id, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}