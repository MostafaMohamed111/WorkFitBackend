using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Identity.Features.AssignTeamLead;

public sealed record AssignTeamLeadCommand(Guid UserId) : IRequest;
