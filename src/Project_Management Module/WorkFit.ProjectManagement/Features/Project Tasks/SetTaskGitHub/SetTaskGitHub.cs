using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Domain.Entities;
using WorkFit.ProjectManagement.Features.Exceptions;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.SetTaskGitHub;

public sealed record SetTaskGitHubRequest(
    long? GitHubRepositoryId,
    string? GitHubRepositoryName,
    string? GitHubBranchName,
    int? GitHubPullRequestNumber);

public sealed record SetTaskGitHubCommand(
    Guid TaskId,
    long? GitHubRepositoryId,
    string? GitHubRepositoryName,
    string? GitHubBranchName,
    int? GitHubPullRequestNumber) : IRequest<Guid>;

public sealed class SetTaskGitHubCommandHandler : IRequestHandler<SetTaskGitHubCommand, Guid>
{
    private readonly WorkFitProjectDbContext _context;
    private readonly ICurrentUserContext _currentUser;

    public SetTaskGitHubCommandHandler(WorkFitProjectDbContext context, ICurrentUserContext currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(SetTaskGitHubCommand command, CancellationToken ct)
    {
        var task = await _context.ProjectTasks
            .FirstOrDefaultAsync(x => x.Id == command.TaskId, ct)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, "ProjectTask", command.TaskId);

        var actorId = _currentUser.GetUserId(ct);
        if (actorId != task.CreatedById)
        {
            throw new UnAuthorizedTeamLeadAccessException(actorId);
        }

        var taskGitHub = await _context.TaskGitHubs.FirstOrDefaultAsync(x => x.TaskId == command.TaskId, ct);
        if (taskGitHub is null)
        {
            taskGitHub = TaskGitHub.Create(
                command.TaskId,
                command.GitHubRepositoryId,
                command.GitHubRepositoryName,
                command.GitHubBranchName,
                command.GitHubPullRequestNumber);

            await _context.TaskGitHubs.AddAsync(taskGitHub, ct);
        }
        else
        {
            taskGitHub.Update(
                command.GitHubRepositoryId,
                command.GitHubRepositoryName,
                command.GitHubBranchName,
                command.GitHubPullRequestNumber);
        }

        await _context.SaveChangesAsync(ct);

        return task.Id;
    }
}

public sealed class SetTaskGitHubEndpoint : Endpoint<SetTaskGitHubRequest, Guid>
{
    private readonly IMediator _mediator;

    public SetTaskGitHubEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/tasks/{id}/github");
        Options(x => x.WithTags("Project Management"));
        Roles("TeamLeader");
    }

    public override async Task HandleAsync(SetTaskGitHubRequest req, CancellationToken ct)
    {
        var taskId = Route<Guid>("id");
        var result = await _mediator.Send(
            new SetTaskGitHubCommand(taskId, req.GitHubRepositoryId, req.GitHubRepositoryName, req.GitHubBranchName, req.GitHubPullRequestNumber),
            ct);
        await Send.OkAsync(result, ct);
    }
}
