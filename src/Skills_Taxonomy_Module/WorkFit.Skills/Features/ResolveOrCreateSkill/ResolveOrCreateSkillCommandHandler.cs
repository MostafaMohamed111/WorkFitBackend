using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.Skills.Contracts.Dtos;
using WorkFit.Skills.Domain.Entities;
using WorkFit.Skills.Infrastructure.Data;
using WorkFit.Skills.Infrastructure.Similarity;

namespace WorkFit.Skills.Features.ResolveOrCreateSkill;

public sealed class ResolveOrCreateSkillCommandHandler
    : IRequestHandler<ResolveOrCreateSkillCommand, SkillResolutionResult>
{
    private const double SimilarityConfidenceThreshold = 0.85;

    private readonly WorkFitSkillsDbContext _db;
    private readonly ISkillSimilarityService _similarityService;

    public ResolveOrCreateSkillCommandHandler(
        WorkFitSkillsDbContext db,
        ISkillSimilarityService similarityService)
    {
        _db = db;
        _similarityService = similarityService;
    }

    public async Task<SkillResolutionResult> Handle(ResolveOrCreateSkillCommand request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RawSkillName))
            throw new FeildIsNullOrEmptyException("Skills", nameof(Skill), nameof(request.RawSkillName));

        var normalized = Skill.Normalize(request.RawSkillName);

        var exactMatch = await _db.Skills
            .Where(s => s.NormalizedName == normalized)
            .Select(s => new { s.Id, s.Name })
            .FirstOrDefaultAsync(cancellationToken);
        if (exactMatch is not null)
            return new SkillResolutionResult(exactMatch.Id, exactMatch.Name, SkillResolutionMethod.ExactMatch);

        var synonymMatch = await _db.SkillSynonyms
            .Where(s => s.NormalizedAlias == normalized)
            .Join(_db.Skills, syn => syn.SkillId, sk => sk.Id, (syn, sk) => new { sk.Id, sk.Name })
            .FirstOrDefaultAsync(cancellationToken);
        if (synonymMatch is not null)
            return new SkillResolutionResult(synonymMatch.Id, synonymMatch.Name, SkillResolutionMethod.SynonymMatch);

        var similarity = await _similarityService.FindBestMatchAsync(request.RawSkillName, cancellationToken);
        if (similarity is not null && similarity.Confidence >= SimilarityConfidenceThreshold)
        {
            var matchedSkill = await _db.Skills
                .Include(s => s.Synonyms)
                .FirstAsync(s => s.Id == similarity.SkillId, cancellationToken);

            matchedSkill.AddSynonym(request.RawSkillName); 
            await _db.SaveChangesAsync(cancellationToken);
            return new SkillResolutionResult(matchedSkill.Id, matchedSkill.Name, SkillResolutionMethod.SimilarityMatch);
        }

        var newSkill = Skill.Create(request.RawSkillName);
        _db.Skills.Add(newSkill);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return new SkillResolutionResult(newSkill.Id, newSkill.Name, SkillResolutionMethod.NewlyCreated);
        }
        catch (DbUpdateException)
        {
            _db.Entry(newSkill).State = EntityState.Detached;

            var winner = await _db.Skills
                .Where(s => s.NormalizedName == normalized)
                .Select(s => new { s.Id, s.Name })
                .FirstAsync(cancellationToken);
            return new SkillResolutionResult(winner.Id, winner.Name, SkillResolutionMethod.ExactMatch);
        }
    }
}