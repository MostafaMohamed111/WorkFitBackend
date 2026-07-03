using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Features.Project.GetProjects;

namespace WorkFit.ProjectManagement.Infrastructure.Data.Repositories;



public sealed class ProjectRepository : IProjectRepository
{
    private readonly WorkFitProjectDbContext _context;

    public ProjectRepository(WorkFitProjectDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProjectListItemDto>> GetProjectsAsync(
        string? status,
        Guid? departmentId,
        int page,
        int limit,
        CancellationToken ct)
    {
        IQueryable<Project> query = _context.Projects
            .AsNoTracking()
            .Include(x => x.Assignments)
            .Include(x => x.Tasks);

        if (Enum.TryParse<ProjectStatus>(status, true, out var projectStatus))
        {
            query = query.Where(x => x.Status == projectStatus);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(x => x.DepartmentId == departmentId.Value);
        }

        return await query
            .Skip((page - 1) * limit)
            .Take(limit)
           .Select(x => new ProjectListItemDto(
            x.Id,
            x.Name,
            x.DepartmentId,
            x.Status,
            x.StartDate,
            x.EndDate,
            x.Assignments.Count(a => a.IsActive),
            x.Tasks.Count))
            .ToListAsync(ct);
    }

    public async Task AddAsync(Project project, CancellationToken ct)
    {
        await _context.Projects.AddAsync(project, ct);
    }

    public Task UpdateAsync(Project project, CancellationToken ct)
    {
        _context.Projects.Update(project);
        return Task.CompletedTask;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ct)
    {
        var project = await _context.Projects
            .Include(x => x.Assignments)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (project is null)
            throw new KeyNotFoundException($"Project '{id}' was not found.");

        // Call a domain method here instead of setting private properties.
        // Example:
        // project.Archive();

        // If you don't have domain methods yet, leave this method empty
        // until they are added.
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        return _context.Projects.AnyAsync(x => x.Id == id, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _context.SaveChangesAsync(ct);
    }
}

