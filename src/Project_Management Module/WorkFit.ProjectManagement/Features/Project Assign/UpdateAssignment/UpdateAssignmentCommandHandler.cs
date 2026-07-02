using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.UpdateAssignment;

public sealed class UpdateAssignmentCommandHandler : IRequestHandler<UpdateAssignmentCommand>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public UpdateAssignmentCommandHandler(WorkFitProjectDbContext context, ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateAssignmentCommand command, CancellationToken ct)
    {
        var assignment = await _context.ProjectAssignments
            .FirstOrDefaultAsync(a => a.Id == command.AssignmentId && a.ProjectId == command.ProjectId, ct);

        if (assignment is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectAssignment", command.AssignmentId);

        if (!assignment.IsActive)
            throw new FeatureException(ModuleMarker.ModuleName, "ASSIGNMENT_ALREADY_ENDED", "Cannot update an ended assignment.");

        assignment.UpdateAllocation(command.AllocationPercentage, command.RoleOnProject, command.EndDate);

        var actorId = _currentUser.GetUserId(ct);

        var activityLog = Domain.Entities.ProjectActivityLog.Create(
            command.ProjectId,
            actorId,
            ActivityActions.MemberUpdated,
            ActivityEntityType.Assignment,
            entityId: assignment.Id);

        _context.ProjectActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync(ct);
    }
}
