using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.ArchiveProject;

public sealed record ArchiveProjectCommand(Guid Id) : IRequest<Guid>;
