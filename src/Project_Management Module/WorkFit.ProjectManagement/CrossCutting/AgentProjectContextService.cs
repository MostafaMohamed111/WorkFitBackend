using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.ProjectManagement.Contracts.Agent;
using WorkFit.ProjectManagement.Infrastructure;
using WorkFit.SharedKernel.ICurrentUser;

namespace WorkFit.ProjectManagement.CrossCutting;

internal sealed class AgentProjectContextService(
    WorkFitProjectDbContext db,
    ICurrentUserContext currentUser,
    IGetOrganizationIdService organizations) : IAgentProjectContextService
{
    public async Task<IReadOnlyList<AgentProjectContextDto>> GetVisibleProjectsAsync(
        Guid? projectId,
        CancellationToken cancellationToken = default)
    {
        var userId = currentUser.GetUserId(cancellationToken);
        var organizationId = await organizations.GetOrganizationIdAsync(userId, cancellationToken);
        var query = db.Projects
            .AsNoTracking()
            .Where(project => project.OrganizationId == organizationId);

        if (projectId.HasValue)
        {
            query = query.Where(project => project.Id == projectId.Value);
        }

        var rawProjects = await query
            .OrderBy(project => project.Name)
            .Take(25)
            .Select(project => new
            {
                project.Id,
                project.OrganizationId,
                project.Name,
                project.Description,
                Status = project.Status.ToString(),
                project.TeamLeaderId,
                MemberIds = project.Members.Select(member => member.EmployeeProfileId).ToList(),
                TaskAssigneeIds = project.Tasks
                    .Where(task => task.AssignedEmployeeId.HasValue)
                    .Select(task => task.AssignedEmployeeId!.Value)
                    .ToList(),
                Tasks = project.Tasks
                    .Where(task => task.DeletedAt == null)
                    .OrderBy(task => task.Title)
                    .Take(100)
                    .Select(task => new
                    {
                        task.Id,
                        task.Title,
                        Status = task.Status.ToString(),
                        Priority = task.Priority.ToString(),
                        task.AssignedEmployeeId,
                        task.AllocationPercentage,
                        task.DueDate
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return rawProjects.Select(p => new AgentProjectContextDto(
            p.Id,
            p.OrganizationId,
            p.Name,
            p.Description,
            p.Status,
            p.TeamLeaderId,
            p.MemberIds.Concat(p.TaskAssigneeIds).Distinct().ToArray(),
            p.Tasks.Select(t => new AgentProjectTaskDto(
                t.Id,
                t.Title,
                t.Status,
                t.Priority,
                t.AssignedEmployeeId,
                t.AllocationPercentage,
                t.DueDate
            )).ToArray()
        )).ToList();
    }
}
