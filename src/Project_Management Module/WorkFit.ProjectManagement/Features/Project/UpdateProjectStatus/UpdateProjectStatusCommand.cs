using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.UpdateProjectStatus;

public sealed record UpdateProjectStatusCommand(Guid Id, string Status) : IRequest<ProjectStatusDto?>;
