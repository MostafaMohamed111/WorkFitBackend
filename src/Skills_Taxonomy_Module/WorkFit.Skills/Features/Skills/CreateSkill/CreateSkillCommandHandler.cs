using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.Skills.Domain.Entities;
using WorkFit.Skills.Domain.Exceptions;
using WorkFit.Skills.Infrastructure.Data;

namespace WorkFit.Skills.Features.Skills.CreateSkill;

public sealed class CreateSkillCommandHandler : IRequestHandler<CreateSkillCommand, SkillDto>
{
    private readonly WorkFitSkillsDbContext _db;

    public CreateSkillCommandHandler(WorkFitSkillsDbContext db)
    {
        _db = db;
    }

    public async Task<SkillDto> Handle(CreateSkillCommand request, CancellationToken cancellationToken)
    {
        // 1. Check for duplicate skill name (same organization scope)
        var exists = await _db.Skills
            .AnyAsync(s =>
                s.NormalizedName == request.Name.ToUpperInvariant() &&
                s.OrganizationId == request.OrganizationId &&
                !s.IsDeleted,
                cancellationToken);

        if (exists)
            throw new DuplicateSkillException(request.Name);

        // 2. Validate Category exists (if provided)
        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _db.SkillCategories
                .AnyAsync(c => c.Id == request.CategoryId && c.IsActive, cancellationToken);

            if (!categoryExists)
                throw new FeatureException(
                    ModuleMarker.ModuleName,
                    "CATEGORY_NOT_FOUND",
                    $"Category with ID '{request.CategoryId}' not found or inactive.",
                    "The specified category does not exist."
                );
        }

        // 3. Validate Group exists (if provided)
        if (request.GroupId.HasValue)
        {
            var groupExists = await _db.SkillGroups
                .AnyAsync(g => g.Id == request.GroupId && g.IsActive, cancellationToken);

            if (!groupExists)
                throw new FeatureException(
                    ModuleMarker.ModuleName,
                    "GROUP_NOT_FOUND",
                    $"Group with ID '{request.GroupId}' not found or inactive.",
                    "The specified group does not exist."
                );
        }

        // 4. Validate Parent Skill exists (if provided)
        if (request.ParentSkillId.HasValue)
        {
            var parentExists = await _db.Skills
                .AnyAsync(s => s.Id == request.ParentSkillId && !s.IsDeleted, cancellationToken);

            if (!parentExists)
                throw new FeatureException(
                    ModuleMarker.ModuleName,
                    "PARENT_SKILL_NOT_FOUND",
                    $"Parent skill with ID '{request.ParentSkillId}' not found.",
                    "The specified parent skill does not exist."
                );
        }

        // 5. Create the skill using the factory method
        var skill = Skill.Create(
            request.Name,
            request.Description,
            request.Origin,
            request.OrganizationId,
            request.CategoryId,
            request.GroupId,
            request.ParentSkillId,
            request.EstimatedTimeToLearn
        );

        // 6. Save to database
        await _db.Skills.AddAsync(skill, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        // 7. Return the DTO
        return new SkillDto(
            skill.Id,
            skill.Name,
            skill.NormalizedName,
            skill.Description,
            skill.Origin,
            skill.OrganizationId,
            skill.CategoryId,
            skill.GroupId,
            skill.ParentSkillId,
            skill.IsDeleted,
            skill.EstimatedTimeToLearn,
            skill.CreatedAt,
            skill.UpdatedAt
        );
    }
}