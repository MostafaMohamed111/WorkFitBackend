using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.CrossCutting;
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
    private readonly IGitHubProjectProvisioningService _gitHubProvisioningService;
    private readonly IMediator _mediator;

    public CreateTaskCommandHandler(
        WorkFitProjectDbContext context,
        ICurrentUserContext currentUser,
        IGitHubProjectProvisioningService gitHubProvisioningService,
        IMediator mediator)
    {
        _context = context;
        _currentUser = currentUser;
        _gitHubProvisioningService = gitHubProvisioningService;
        _mediator = mediator;
    }

    public async Task<Guid> Handle(CreateTaskCommand command, CancellationToken ct)
    {
        var project = await _context.Projects.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, ct);
        if (project is null)
            throw new EntityNotFoundException(ModuleMarker.ModuleName, "Project", command.ProjectId);

        if (!project.GitHubRepositoryId.HasValue)
        {
            throw new InvalidOperationException("The project does not have a GitHub repository provisioned yet.");
        }

        var actorId = _currentUser.GetUserId(ct);
        if(actorId != project.TeamLeaderId)
            throw new UnAuthorizedTeamLeadAccessException(actorId);

        var taskExists = await _context.ProjectTasks
            .AsNoTracking()
            .AnyAsync(t => t.ProjectId == command.ProjectId && t.Title == command.Title, ct);

        if (taskExists)
            throw new InvalidOperationException($"Task '{command.Title}' already exists for project '{command.ProjectId}'.");

        if (command.AssigneeId.HasValue && command.AllocationPercentage == 0)
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "ASSIGNED_TASK_REQUIRES_ALLOCATION",
                "An assigned task must have a positive allocation percentage.",
                "Set a positive allocation percentage for the assigned task.");

        if (command.AssigneeId.HasValue)
        {
            await TaskAllocationCapacityValidator.ValidateAsync(
                _context,
                command.AssigneeId.Value,
                command.AllocationPercentage,
                excludedTaskId: null,
                ct);
        }

        var task = ProjectTask.Create(
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

        var branch = await _gitHubProvisioningService.CreateTaskBranchAsync(
            project.OrganizationId,
            project.Id,
            project.GitHubRepositoryName,
            project.Name,
            command.Title,
            task.Id,
            ct);

        task.SetSource(SourceSystem.GitHub.ToString(), branch.Name);
        task.SetGitHubBranchName(branch.Name);
        task.SetGitHubBranchNodeId(branch.NodeId);

        await _context.ProjectTasks.AddAsync(task, ct);

        await _context.SaveChangesAsync(ct);
        await ProjectTaskStateEventPublisher.PublishAsync(_context, _mediator, task, "Created", ct);

        return task.Id;
    }
}
