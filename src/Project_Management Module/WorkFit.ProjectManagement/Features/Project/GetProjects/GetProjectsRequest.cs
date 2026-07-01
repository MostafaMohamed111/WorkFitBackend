using System;
using System.Collections.Generic;
using System.Text;

namespace WorkFit.ProjectManagement.Features.Project.GetProjects;

public sealed record GetProjectsRequest(
    string? Status,
    Guid? DepartmentId,
    int Page = 1,
    int Limit = 20);