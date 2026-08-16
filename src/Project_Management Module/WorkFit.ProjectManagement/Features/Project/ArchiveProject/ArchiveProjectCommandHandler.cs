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
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project is null)
            return request.Id;

        ProjectAccessGuard.EnsureAuthorized(project, _currentUser, cancellationToken);

        // Execute direct SQL cleanup in exact dependency order using matching schema table names
        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [workflow].[DeveloperInvitations] WHERE ProjectId = {0}",
                new object[] { request.Id },
                cancellationToken);
        }
        catch { }

        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [ProjectManagement].[project_activity_logs] WHERE ProjectId = {0}",
                new object[] { request.Id },
                cancellationToken);
        }
        catch { }

        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [ProjectManagement].[ProjectRequiredSkills] WHERE ProjectId = {0}",
                new object[] { request.Id },
                cancellationToken);
        }
        catch { }

        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [ProjectManagement].[ProjectMembers] WHERE ProjectId = {0}",
                new object[] { request.Id },
                cancellationToken);
        }
        catch { }

        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [ProjectManagement].[task_github] WHERE TaskId IN (SELECT Id FROM [ProjectManagement].[tasks] WHERE ProjectId = {0})",
                new object[] { request.Id },
                cancellationToken);
        }
        catch { }

        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "DELETE FROM [ProjectManagement].[tasks] WHERE ProjectId = {0}",
                new object[] { request.Id },
                cancellationToken);
        }
        catch { }

        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM [ProjectManagement].[projects] WHERE Id = {0}",
            new object[] { request.Id },
            cancellationToken);

        return request.Id;
    }
}
