using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.TalentManagement.Features.GetSkillConfidenceChange;

public sealed record GetSkillConfidenceChangeCommand(Guid Id, bool IsPrivilegedCaller) : IRequest<SkillConfidenceChangeDto>;