using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Features.Common;
using WorkFit.ProjectManagement.Features.Project.GetProjectById;
using WorkFit.ProjectManagement.Features.Project.Queries.Dtos;

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
        Guid? organizationId,
        int page,
        int limit,
        CancellationToken ct)
    {
        IQueryable<Project> query = _context.Projects
            .AsNoTracking()
            .Include(x => x.Tasks);

        if (Enum.TryParse<ProjectStatus>(status, true, out var projectStatus))
        {
            query = query.Where(x => x.Status == projectStatus);
        }

        if (organizationId.HasValue)
        {
            query = query.Where(x => x.OrganizationId == organizationId.Value);
        }

        return await query
            .Skip((page - 1) * limit)
            .Take(limit)
           .Select(x => new ProjectListItemDto(
            x.Id,
            x.Name,
            x.OrganizationId,
            x.Status,
            x.StartDate,
            x.EndDate,
            x.AssignedEmployees.Count(),
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
            project.OrganizationId,
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
        return _context.Projects.AsTracking()
            .Include(p => p.RequiredSkills)
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

