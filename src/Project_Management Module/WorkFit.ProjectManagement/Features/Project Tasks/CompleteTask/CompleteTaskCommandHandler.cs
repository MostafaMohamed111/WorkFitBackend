using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.CompleteTask;
public sealed class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, CompleteTaskResponse>
{
    private readonly WorkFitProjectDbContext _context;

    public CompleteTaskCommandHandler(WorkFitProjectDbContext context)
    {
        _context = context;
    }

    public async Task<CompleteTaskResponse> Handle(CompleteTaskCommand command, CancellationToken ct)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == command.TaskId, ct);
        if (task is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);

        task.Complete();

        await _context.SaveChangesAsync(ct);

        return new CompleteTaskResponse(task.Id, task.Status.ToString(), task.CompletedAt!.Value);
    }
}