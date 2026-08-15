using WorkFit.ProjectManagement.Features.Project.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.Queries.GetProjectsForTeamLead;

internal sealed record GetProjectsForTeamLeadQuery(string? Status)
    : IRequest<IReadOnlyList<ProjectListItemDto>>;

internal sealed record GetProjectsForTeamLeadRequest(string? Status);
