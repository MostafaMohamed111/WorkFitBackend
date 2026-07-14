using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Contracts.IntegrationEvents;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.CompleteTask;
public sealed class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, CompleteTaskResponse>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly IMediator _mediator;

    public CompleteTaskCommandHandler(WorkFitProjectDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<CompleteTaskResponse> Handle(CompleteTaskCommand command, CancellationToken ct)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == command.TaskId, ct);
        if (task is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);

        task.Complete();

        await _context.SaveChangesAsync(ct);
        await _mediator.Publish(new TaskCompletedIntegrationEvent(
            task.Id, task.AssigneeId!.Value, task.AllocationPercentage), ct);


        return new CompleteTaskResponse(task.Id, task.Status.ToString(), task.CompletedAt!.Value);
    }
}