
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.ProjectManagement.Features.Project.GetProjectById;

public sealed record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDetailDto?>;
