namespace WorkFit.ProjectManagement.Features.Project.Queries.GetProjects;

public sealed record GetProjectsRequest(
    string? Status,
    Guid? OrganizationId,
    int Page = 1,
    int Limit = 20);