using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Features.Exceptions;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using TaskType = WorkFit.ProjectManagement.Domain.Enums.TaskType;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.CreateTask;

public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Guid>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public CreateTaskCommandHandler(WorkFitProjectDbContext context, ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateTaskCommand command, CancellationToken ct)
    {
        var project = await _context.Projects.AsTracking()
            .Include(p => p.Tasks)
            .Include(p => p.AssignedEmployees)
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, ct);
        if (project is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Project", command.ProjectId);

        var actorId = _currentUser.GetUserId(ct);
        if(actorId != project.TeamLeaderId)
            throw new UnAuthorizedTeamLeadAccessException(actorId);

        var task = project.CreateTask(
            command.ProjectId,
            command.Title,
            command.Description,
            command.TaskType ?? TaskType.Task,
            command.Priority ?? TaskPriority.Medium,
            actorId,
            command.AssigneeId,
            command.StoryPoints,
            command.DueDate,
            command.AllocationPercentage);

        await _context.SaveChangesAsync(ct);

        return task.Id;
    }
}