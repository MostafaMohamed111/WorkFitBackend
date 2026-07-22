using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Features.Exceptions;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.DeleteTask;

public sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public DeleteTaskCommandHandler(WorkFitProjectDbContext context,
        ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteTaskCommand command, CancellationToken ct)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.Id == command.TaskId, ct);
        if (task is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);

        var actorId = _currentUser.GetUserId(ct);
        if (actorId != task.CreatedById)
            throw new UnAuthorizedTeamLeadAccessException(actorId);

        task.Delete(); 

        await _context.SaveChangesAsync(ct);

    }
}