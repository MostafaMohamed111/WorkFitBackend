using WorkFit.SharedKernel.MediatorContract;
using WorkFit.Skills.Contracts.Dtos;

namespace WorkFit.Skills.Features.ResolveOrCreateSkill;

public sealed record ResolveOrCreateSkillCommand(string RawSkillName) : IRequest<SkillResolutionResult>;