using System;
using System.Collections.Generic;
using System.Text;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.GetProjects; 

public sealed record GetProjectsQuery(
    string? Status,
    Guid? DepartmentId,
    int Page,
    int Limit)
    : IRequest<IReadOnlyList<ProjectListItemDto>>;  