namespace WorkFit.ProjectManagement.Features.Project_Tasks.CompleteTask;

public sealed record CompleteTaskResponse(Guid Id, string Status, DateTimeOffset CompletedAt);