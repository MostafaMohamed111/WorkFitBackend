using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Features.Common;
using WorkFit.ProjectManagement.Features.Exceptions;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.ArchiveProject;

public sealed class ArchiveProjectCommandHandler : IRequestHandler<ArchiveProjectCommand, Guid>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public ArchiveProjectCommandHandler(WorkFitProjectDbContext context, ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project is null)
            return request.Id;

        ProjectAccessGuard.EnsureAuthorized(project, _currentUser, cancellationToken);

        // 1. Delete associated developer invitations from workflow schema
        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [workflow].[DeveloperInvitations] WHERE ProjectId = {0}",
                request.Id,
                cancellationToken);
        }
        catch
        {
            // Fallback if workflow schema or table does not exist
        }

        // 2. Delete all ProjectActivityLogs associated with this project
        var activityLogs = await _context.ProjectActivityLogs
            .Where(l => l.ProjectId == request.Id)
            .ToListAsync(cancellationToken);
        if (activityLogs.Count > 0)
        {
            _context.ProjectActivityLogs.RemoveRange(activityLogs);
        }

        // 3. Delete all ProjectRequiredSkills associated with this project
        var skills = await _context.ProjectRequiredSkills
            .Where(s => s.ProjectId == request.Id)
            .ToListAsync(cancellationToken);
        if (skills.Count > 0)
        {
            _context.ProjectRequiredSkills.RemoveRange(skills);
        }

        // 4. Remove all ProjectMembers associated with this project
        var members = await _context.ProjectMembers
            .Where(m => m.ProjectId == request.Id)
            .ToListAsync(cancellationToken);
        if (members.Count > 0)
        {
            _context.ProjectMembers.RemoveRange(members);
        }

        // 5. Remove all ProjectTasks associated with this project
        var tasks = await _context.ProjectTasks
            .Where(t => t.ProjectId == request.Id)
            .ToListAsync(cancellationToken);
        if (tasks.Count > 0)
        {
            _context.ProjectTasks.RemoveRange(tasks);
        }

        // 6. Delete the Project entity itself from the database
        _context.Projects.Remove(project);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            var entityName = exception.Entries.FirstOrDefault()?.Metadata.ClrType.Name
                ?? nameof(Domain.Entities.Project);

            throw new ConcurrencyConflictException(
                ModuleMarker.ModuleName,
                entityName,
                request.Id,
                exception);
        }

        return request.Id;
    }
}
