using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Domain.Enums;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using TaskType = WorkFit.ProjectManagement.Domain.Enums.TaskType;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.CreateTask;

public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, CreateTaskResponse>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public CreateTaskCommandHandler(WorkFitProjectDbContext context, ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CreateTaskResponse> Handle(CreateTaskCommand command, CancellationToken ct)
    {
        var project = await _context.Projects.FindAsync(new object[] { command.ProjectId }, ct);
        if (project is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Project", command.ProjectId);

        if (command.AssigneeId.HasValue)
        {
            var isMemberOfProject = await _context.ProjectAssignments.AnyAsync(
                a => a.ProjectId == command.ProjectId &&
                     a.EmployeeId == command.AssigneeId.Value &&
                     a.IsActive, ct);

            if (!isMemberOfProject)
                throw new FeatureException(
                    ModuleMarker.ModuleName,
                    "EMPLOYEE_NOT_ASSIGNED_TO_PROJECT",
                    $"Employee '{command.AssigneeId}' is not an active member of project '{command.ProjectId}'.",
                    "You can only assign a task to an employee who is a member of this project.");
        }

        var actorId = _currentUser.GetUserId(ct);

        var task = ProjectTask.Create(
            command.ProjectId,
            command.Title,
            command.Description,
            command.TaskType ?? TaskType.Task,
            command.Priority ?? TaskPriority.Medium,
            actorId,
            command.AssigneeId,
            command.StoryPoints,
            command.DueDate);

        _context.ProjectTasks.Add(task);

        var activityLog = ProjectActivityLog.Create(
            command.ProjectId,
            actorId,
            ActivityActions.TaskCreated,
            ActivityEntityType.Task,
            entityId: task.Id);

        _context.ProjectActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync(ct);

        return new CreateTaskResponse(task.Id, task.Title, task.Status.ToString(), DateTimeOffset.UtcNow);
    }
}