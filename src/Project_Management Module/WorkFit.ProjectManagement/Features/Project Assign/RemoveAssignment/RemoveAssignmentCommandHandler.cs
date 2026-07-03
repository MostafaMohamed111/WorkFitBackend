using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.RemoveAssignment;

public sealed class RemoveAssignmentCommandHandler : IRequestHandler<RemoveAssignmentCommand>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public RemoveAssignmentCommandHandler(WorkFitProjectDbContext context, ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(RemoveAssignmentCommand command, CancellationToken ct)
    {
        var assignment = await _context.ProjectAssignments
            .FirstOrDefaultAsync(a => a.Id == command.AssignmentId && a.ProjectId == command.ProjectId, ct);

        if (assignment is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectAssignment", command.AssignmentId);

        if (!assignment.IsActive)
            throw new FeatureException(ModuleMarker.ModuleName, "ASSIGNMENT_ALREADY_ENDED", "Assignment has already been ended.");

        assignment.EndAssignment();

        var actorId = _currentUser.GetUserId(ct);

        var activityLog = ProjectActivityLog.Create(
            command.ProjectId,
            actorId,
            ActivityActions.MemberRemoved,
            ActivityEntityType.Assignment,
            entityId: assignment.Id);

        _context.ProjectActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync(ct);
    }
}
