using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Contracts.IntegrationEvents;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.AssignTask;

public sealed class AssignTaskCommandHandler : IRequestHandler<AssignTaskCommand, AssignTaskResponse>
{
    private readonly WorkFitProjectDbContext _context;

    private readonly IMediator _mediator;

    public AssignTaskCommandHandler(WorkFitProjectDbContext context,
            IMediator mediator
        )
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<AssignTaskResponse> Handle(AssignTaskCommand command, CancellationToken ct)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == command.TaskId, ct);
        if (task is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);


        var isMemberOfProject = await _context.ProjectAssignments.AnyAsync(
            a => a.ProjectId == task.ProjectId &&
                 a.EmployeeId == command.AssigneeId &&
                 a.IsActive, ct);

        if (!isMemberOfProject)
        {
            var newAssignment = ProjectAssignment.Create(
                projectId: task.ProjectId,
                employeeId: command.AssigneeId,
                roleOnProject: null,
                allocationPercentage: 0,
                startDate: DateOnly.FromDateTime(DateTime.UtcNow),
                endDate: null);

            _context.ProjectAssignments.Add(newAssignment);
        
        }

        task.Assign(command.AssigneeId);

        if (command.AllocationPercentage.HasValue)
            task.SetAllocationPercentage(command.AllocationPercentage.Value);


        await _context.SaveChangesAsync(ct);

        if (task.IsActive)
        {
            await _mediator.Publish(new TaskAssignedIntegrationEvent(
                task.Id, task.AssigneeId!.Value, task.AllocationPercentage), ct);
        }

        return new AssignTaskResponse(task.Id, task.AssigneeId!.Value);
    }
}