namespace WorkFit.ProjectManagement.Features.Project_Tasks.CreateTask;

public sealed record CreateTaskResponse(Guid Id, string Title, string Status, DateTimeOffset CreatedAt);