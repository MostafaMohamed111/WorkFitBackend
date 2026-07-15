using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Features.Common;
using WorkFit.ProjectManagement.Features.Project.GetProjectById;
using WorkFit.ProjectManagement.Features.Project.GetProjectDomains;
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
    public async Task<IReadOnlyList<ProjectDomainDto>> GetDomainsAsync(Guid projectId, CancellationToken ct)
    {
        var domainIds = await _context.ProjectDomains
            .AsNoTracking()
            .Where(d => d.ProjectId == projectId)
            .Select(d => d.DomainId)
            .ToArrayAsync(ct);

        if (domainIds.Length == 0)
            return Array.Empty<ProjectDomainDto>();

        return await _context.Set<OrgDomain>()
            .Where(x => domainIds.Contains(x.Id))
            .Select(x => new ProjectDomainDto(
                x.Id,
                x.Name))
            .ToListAsync(ct);
    }
    public Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        return _context.Projects.AnyAsync(x => x.Id == id, ct);
    }
    public Task<bool> DomainTagExistsAsync(Guid projectId, Guid domainId, CancellationToken ct)
    {
        return _context.ProjectDomains.AsNoTracking()
            .AnyAsync(d => d.ProjectId == projectId && d.DomainId == domainId, ct);
    }

    public async Task AddDomainAsync(ProjectDomain projectDomain, CancellationToken ct)
    {
        await _context.ProjectDomains.AddAsync(projectDomain, ct);
    }
    public async Task<bool> RemoveDomainAsync(Guid projectId, Guid domainId, CancellationToken ct)
    {
        var existing = await _context.ProjectDomains
            .FirstOrDefaultAsync(d => d.ProjectId == projectId && d.DomainId == domainId, ct);

        if (existing is null)
            return false;

        _context.ProjectDomains.Remove(existing);
        return true;
    }
    public async Task<ProjectDetailDto?> GetDetailByIdAsync(Guid id, CancellationToken ct)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.RequiredSkills)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (project is null)
            return null;

        var requiredSkillDtos = project.RequiredSkills
            .Select(s => new RequiredSkillDto(
                s.SkillId,
                "Unknown", // Placeholder until Skills module exists
                s.Level.ToApiString(),
                s.Priority))
            .ToList();

        const double coveragePct = 0d;

        return new ProjectDetailDto(
            project.Id,
            project.Name,
            project.Description,
            project.DepartmentId,
            project.Status.ToApiString(),
            project.TeamLeaderId,
            project.StartDate,
            project.EndDate,
            requiredSkillDtos,
            project.SourceSystem,
            project.SourceReferenceId,
            project.CreatedAt,
            coveragePct);
    }
    public Task<Project?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return _context.Projects
            .Include(p => p.RequiredSkills)
            .Include(p => p.Domains)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }
    public async Task AddActivityLogAsync(ProjectActivityLog log, CancellationToken ct)
    {
        await _context.ProjectActivityLogs.AddAsync(log, ct);
    }
    public Task SaveChangesAsync(CancellationToken ct)
    {
        return _context.SaveChangesAsync(ct);
    }
}

