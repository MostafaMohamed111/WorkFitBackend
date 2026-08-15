using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.ProjectManagement.Infrastructure;

namespace WorkFit.ProjectManagement.Features.Project_Tasks.GetEmployeeAssignedTasks;

public sealed record DeveloperTaskDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Title,
    string? Description,
    int Status,
    string StatusName,
    int Priority,
    string PriorityName,
    DateOnly? DueDate,
    int StoryPoints,
    Guid? CreatedById
);

public sealed class GetEmployeeAssignedTasksEndpoint : EndpointWithoutRequest<List<DeveloperTaskDto>>
{
    private readonly WorkFitProjectDbContext _context;

    public GetEmployeeAssignedTasksEndpoint(WorkFitProjectDbContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Get("/api/employees/{employeeId}/assigned-tasks");
        Roles("TeamLeader", "OrganizationOwner", "Admin", "SuperAdmin", "Employee");
        Options(x => x.WithTags("Project Management"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var employeeIdRoute = Route<string>("employeeId");
        if (string.IsNullOrEmpty(employeeIdRoute) || !Guid.TryParse(employeeIdRoute, out var employeeId))
        {
            await Send.OkAsync(new List<DeveloperTaskDto>(), ct);
            return;
        }

        var tasks = await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.AssignedEmployeeId == employeeId)
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync(ct);

        if (tasks.Count == 0)
        {
            await Send.OkAsync(new List<DeveloperTaskDto>(), ct);
            return;
        }

        var projectIds = tasks.Select(t => t.ProjectId).Distinct().ToList();
        var projects = await _context.Projects
            .AsNoTracking()
            .Where(p => projectIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, ct);

        var result = tasks.Select(t => new DeveloperTaskDto(
            t.Id,
            t.ProjectId,
            projects.TryGetValue(t.ProjectId, out var pName) ? pName : "Project",
            t.Title,
            t.Description,
            (int)t.Status,
            t.Status.ToString(),
            (int)t.Priority,
            t.Priority.ToString(),
            t.DueDate,
            t.StoryPoints ?? 0,
            t.CreatedById
        )).ToList();

        await Send.OkAsync(result, ct);
    }
}
