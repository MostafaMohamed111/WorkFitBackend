using System;
using System.Collections.Generic;
using System.Text;
using WorkFit.ProjectManagement.Features.Project.Queries.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.Queries.GetProjects; 

public sealed record GetProjectsQuery(
    string? Status,
    Guid? OrganizationId,
    int Page,
    int Limit)
    : IRequest<IReadOnlyList<ProjectListItemDto>>;  