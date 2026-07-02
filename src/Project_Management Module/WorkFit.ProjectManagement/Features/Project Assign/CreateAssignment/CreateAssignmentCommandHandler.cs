using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.CreateAssignment;

public sealed class CreateAssignmentCommandHandler : IRequestHandler<CreateAssignmentCommand, CreateAssignmentResponse>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public CreateAssignmentCommandHandler(WorkFitProjectDbContext context, ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CreateAssignmentResponse> Handle(CreateAssignmentCommand command, CancellationToken ct)
    {
        var project = await _context.Projects.FindAsync(new object[] { command.ProjectId }, ct);
        if (project is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Project", command.ProjectId);

        var assignment = ProjectAssignment.Create(
            command.ProjectId,
            command.EmployeeId,
            command.RoleOnProject,
            command.AllocationPercentage,
            command.StartDate,
            command.EndDate);

        _context.ProjectAssignments.Add(assignment);

        var totalAllocation = await _context.ProjectAssignments
            .Where(a => a.EmployeeId == command.EmployeeId && a.IsActive)
            .SumAsync(a => a.AllocationPercentage, ct);

        totalAllocation += command.AllocationPercentage;

        var warnings = new List<string>();
        if (totalAllocation > 100)
            warnings.Add($"Employee's total allocation across all projects is {totalAllocation}%, which exceeds 100%.");

        var actorId = _currentUser.GetUserId(ct);

        var activityLog = ProjectActivityLog.Create(
            command.ProjectId,
            actorId,
            ActivityActions.MemberAdded,
            ActivityEntityType.Assignment,
            entityId: assignment.Id);

        _context.ProjectActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync(ct);

        return new CreateAssignmentResponse(assignment.Id, warnings.ToArray());
    }
}
