using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.DeleteTask;

public sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, DeleteTaskResponse>
{
    private readonly WorkFitProjectDbContext _context;

    public DeleteTaskCommandHandler(WorkFitProjectDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteTaskResponse> Handle(DeleteTaskCommand command, CancellationToken ct)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == command.TaskId, ct);
        if (task is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);

        task.Delete(); 

        await _context.SaveChangesAsync(ct);

        return new DeleteTaskResponse(true, task.Id);
    }
}