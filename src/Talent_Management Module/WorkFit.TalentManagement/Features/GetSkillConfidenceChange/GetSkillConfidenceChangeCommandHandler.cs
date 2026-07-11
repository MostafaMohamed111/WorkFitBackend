using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Domain.Entities;
using WorkFit.TalentManagement.Infrastructure.Data;

namespace WorkFit.TalentManagement.Features.GetSkillConfidenceChange;

public sealed class GetSkillConfidenceChangeCommandHandler
    : IRequestHandler<GetSkillConfidenceChangeCommand, SkillConfidenceChangeDto>
{
    private readonly TalentDbContext _db;
    private readonly ICurrentUserContext _currentUser;

    public GetSkillConfidenceChangeCommandHandler(TalentDbContext db, ICurrentUserContext currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<SkillConfidenceChangeDto> Handle(GetSkillConfidenceChangeCommand request, CancellationToken cancellationToken = default)
    {
        var organizationId = _currentUser.GetOrganizationId(cancellationToken);

        var result = await _db.SkillConfidenceChanges
            .Include(scc => scc.ConfidenceEvidences)
            .Where(scc => scc.Id == request.Id
                          && !scc.IsDeleted)
            .Select(scc => new SkillConfidenceChangeDto(
                scc.Id,
                scc.EmployeeSkillId,
                scc.AssessmentId,
                scc.OldScore,
                scc.NewScore,
                scc.CreatedAt,
                scc.ConfidenceEvidences
                    .Where(e => !e.IsDeleted)
                    .OrderBy(e => e.CreatedAt)
                    .Select(e => new ConfidenceEvidenceDto(
                        e.Id,
                        e.Source,
                        e.Details,
                        e.CreatedAt))
                    .ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (result is null)
            throw new EntityNotFoundException(
                ModuleMarker.ModuleName,
                nameof(SkillConfidenceChange),
                request.Id);

        return result;
    }
}