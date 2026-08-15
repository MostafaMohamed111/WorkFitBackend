using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WorkFit.Organizations.Contracts.OrganizationServices;
using WorkFit.ProjectManagement.Contracts.Membership;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.Employee.ProjectMembers;

public sealed record AddProjectMemberRequest(Guid EmployeeId);

public sealed record ProjectMemberDto(
    Guid Id,
    string Name,
    string? Email,
    string JobTitle,
    bool IsActive,
    int CurrentAllocationPercentage);

public sealed class GetProjectMembersEndpoint : EndpointWithoutRequest<List<ProjectMemberDto>>
{
    private readonly IProjectMembershipService _projects;
    private readonly IGetOrganizationIdService _organizations;
    private readonly ICurrentUserContext _currentUser;
    private readonly TalentDbContext _talent;

    public GetProjectMembersEndpoint(
        IProjectMembershipService projects,
        IGetOrganizationIdService organizations,
        ICurrentUserContext currentUser,
        TalentDbContext talent)
    {
        _projects = projects;
        _organizations = organizations;
        _currentUser = currentUser;
        _talent = talent;
    }

    public override void Configure()
    {
        Get("/api/projects/{projectId}/members");
        Roles("TeamLeader", "OrganizationOwner", "Admin", "SuperAdmin", "Employee");
        Options(x => x.WithTags("Project Management"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var projectId = Route<Guid>("projectId");
        var scope = await _projects.GetInvitationScopeAsync(projectId, ct);
        if (scope is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (!await ProjectMemberAuthorization.IsAuthorizedAsync(
                scope, _projects, _organizations, _currentUser, _talent, ct))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        var memberIds = await _projects.GetMemberIdsAsync(projectId, ct);
        var members = await _talent.EmployeeProfiles
            .AsNoTracking()
            .Where(employee =>
                memberIds.Contains(employee.Id) &&
                !employee.IsDeleted)
            .OrderBy(employee => employee.Name)
            .Select(employee => new ProjectMemberDto(
                employee.Id,
                employee.Name,
                employee.Email,
                employee.JobTitle,
                employee.Status == Domain.Enums.EmployeeProfileStatus.Active,
                employee.CurrentAllocationPercentage))
            .ToListAsync(ct);

        await Send.OkAsync(members, ct);
    }
}

public sealed class AddProjectMemberEndpoint : Endpoint<AddProjectMemberRequest, ProjectMemberDto>
{
    private readonly IProjectMembershipService _projects;
    private readonly IGetOrganizationIdService _organizations;
    private readonly ICurrentUserContext _currentUser;
    private readonly TalentDbContext _talent;

    public AddProjectMemberEndpoint(
        IProjectMembershipService projects,
        IGetOrganizationIdService organizations,
        ICurrentUserContext currentUser,
        TalentDbContext talent)
    {
        _projects = projects;
        _organizations = organizations;
        _currentUser = currentUser;
        _talent = talent;
    }

    public override void Configure()
    {
        Post("/api/projects/{projectId}/members");
        Roles("TeamLeader", "OrganizationOwner", "Admin", "SuperAdmin");
        Options(x => x.WithTags("Project Management"));
    }

    public override async Task HandleAsync(AddProjectMemberRequest req, CancellationToken ct)
    {
        var projectId = Route<Guid>("projectId");
        var scope = await _projects.GetInvitationScopeAsync(projectId, ct);
        if (scope is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (!await ProjectMemberAuthorization.IsAuthorizedAsync(
                scope, _projects, _organizations, _currentUser, _talent, ct))
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        var employee = await _talent.EmployeeProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate =>
                (candidate.Id == req.EmployeeId || candidate.UserId == req.EmployeeId) &&
                candidate.OrganizationId == scope.OrganizationId &&
                !candidate.IsDeleted,
                ct);

        if (employee is null)
        {
            employee = await _talent.EmployeeProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(candidate =>
                    (candidate.Id == req.EmployeeId || candidate.UserId == req.EmployeeId) &&
                    !candidate.IsDeleted,
                    ct);
        }

        if (employee is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await _projects.AddMemberAsync(projectId, employee.Id, scope.OrganizationId, ct);

        await Send.OkAsync(new ProjectMemberDto(
            employee.Id,
            employee.Name,
            employee.Email,
            employee.JobTitle,
            employee.Status == Domain.Enums.EmployeeProfileStatus.Active,
            employee.CurrentAllocationPercentage), ct);
    }
}

file static class ProjectMemberAuthorization
{
    public static async Task<bool> IsAuthorizedAsync(
        ProjectInvitationScope scope,
        IProjectMembershipService projects,
        IGetOrganizationIdService organizations,
        ICurrentUserContext currentUser,
        TalentDbContext talent,
        CancellationToken ct)
    {
        var userId = currentUser.GetUserId(ct);
        var roles = currentUser.GetRoles(ct);
        if (roles.Contains("SuperAdmin")) return true;

        var employee = await talent.EmployeeProfiles
            .AsNoTracking()
            .Where(profile => !profile.IsDeleted && (profile.UserId == userId || profile.Id == userId))
            .Select(profile => new { profile.Id, profile.OrganizationId })
            .FirstOrDefaultAsync(ct);

        if (scope.TeamLeaderId == userId || scope.TeamLeaderId == employee?.Id)
            return true;

        if (roles.Any(role => role is "TeamLeader" or "OrganizationOwner" or "Admin"))
            return true;

        if (roles.Contains("Employee") && employee is not null)
        {
            var memberIds = await projects.GetMemberIdsAsync(scope.ProjectId, ct);
            if (memberIds.Contains(employee.Id)) return true;
        }

        if (employee?.OrganizationId == scope.OrganizationId)
            return true;

        try
        {
            return await organizations.GetOrganizationIdAsync(userId, ct) == scope.OrganizationId;
        }
        catch
        {
            return false;
        }
    }
}
