using System;
using System.Collections.Generic;
using System.Text;
using WorkFit.ProjectManagement.Domain.Enums;

namespace WorkFit.ProjectManagement.Features.Project.Queries.Dtos;

public sealed record ProjectListItemDto(
    Guid Id,
    string Name,
    Guid OrganizationId,
    ProjectStatus Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int MemberCount,
    int TaskCount);