using WorkFit.ProjectManagement.Features.Project.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.Queries.GetProjectsForTeamLead;

internal sealed record  GetProjectsForTeamLeadQuery()
    : IRequest<IReadOnlyList<ProjectListItemDto>>;
