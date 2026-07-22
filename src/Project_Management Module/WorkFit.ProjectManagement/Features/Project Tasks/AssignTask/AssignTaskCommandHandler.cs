using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Contracts.IntegrationEvents;
using WorkFit.ProjectManagement.Features.Exceptions;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.AssignTask;

public sealed class AssignTaskCommandHandler : IRequestHandler<AssignTaskCommand, Guid>
{
    private readonly WorkFitProjectDbContext _context;

    private readonly IMediator _mediator;
    private readonly ICurrentUserContext _currentUser;

    public AssignTaskCommandHandler(WorkFitProjectDbContext context,
            IMediator mediator,
            ICurrentUserContext currentUser
        )
    {
        _context = context;
        _mediator = mediator;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(AssignTaskCommand command, CancellationToken ct)
    {
        var project = await _context.Projects.AsTracking()
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, ct)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, typeof(Domain.Entities.Project).Name, command.ProjectId);

        var actorId = _currentUser.GetUserId(ct);
        if(actorId != project.TeamLeaderId)
            throw new UnAuthorizedTeamLeadAccessException(actorId);

        var task = project.AssignEmployeeForTask(command.TaskId, command.AssigneeId, command.AllocationPercentage);

        await _context.SaveChangesAsync(ct);

       
        await _mediator.Publish(new TaskAssignedIntegrationEvent(
        task.Id, task.AssignedEmployeeId!.Value, task.AllocationPercentage), ct);


        return task.Id;
    }
}