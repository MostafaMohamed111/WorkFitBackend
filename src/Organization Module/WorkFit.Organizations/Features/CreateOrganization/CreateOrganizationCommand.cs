
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Organizations.Features.CreateOrganization;

public sealed record CreateOrganizationCommand(string Name, Guid UserId) : IRequest<Guid>;
