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
        var employeeId = Route<Guid>("employeeId");

        var tasks = await (from t in _context.ProjectTasks.AsNoTracking()
                           join p in _context.Projects.AsNoTracking() on t.ProjectId equals p.Id
                           where t.AssignedEmployeeId == employeeId
                           orderby t.UpdatedAt descending
                           select new DeveloperTaskDto(
                               t.Id,
                               p.Id,
                               p.Name,
                               t.Title,
                               t.Description,
                               (int)t.Status,
                               t.Status.ToString(),
                               (int)t.Priority,
                               t.Priority.ToString(),
                               t.DueDate,
                               t.StoryPoints ?? 0,
                               t.CreatedById
                           )).ToListAsync(ct);

        await Send.OkAsync(tasks, ct);
    }
}
