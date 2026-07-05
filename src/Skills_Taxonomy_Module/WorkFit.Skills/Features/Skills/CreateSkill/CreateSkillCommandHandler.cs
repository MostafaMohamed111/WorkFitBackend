using Microsoft.EntityFrameworkCore;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.Skills.Domain.Entities;
using WorkFit.Skills.Domain.Exceptions;
using WorkFit.Skills.Infrastructure.Data;

namespace WorkFit.Skills.Features.Skills.CreateSkill;

public sealed class CreateSkillCommandHandler : IRequestHandler<CreateSkillCommand, SkillDto>
{
    private readonly WorkFitSkillsDbContext _context;

    public CreateSkillCommandHandler(WorkFitSkillsDbContext context)
    {
        _context = context;
    }

    public async Task<SkillDto> Handle(CreateSkillCommand command, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Skills
            .AnyAsync(s =>
                s.NormalizedName == command.Name.ToUpperInvariant() &&
                s.OrganizationId == command.OrganizationId &&
                !s.IsDeleted,
                cancellationToken);

        if (exists)
            throw new DuplicateSkillException(command.Name);

        if (command.CategoryId.HasValue)
        {
            var categoryExists = await _context.SkillCategories
                .AnyAsync(c => c.Id == command.CategoryId && c.IsActive, cancellationToken);

            if (!categoryExists)
                throw new FeatureException(
                    ModuleMarker.ModuleName,
                    "CATEGORY_NOT_FOUND",
                    $"Category with ID '{command.CategoryId}' not found or inactive.",
                    "The specified category does not exist."
                );
        }

        if (command.GroupId.HasValue)
        {
            var groupExists = await _context.SkillGroups
                .AnyAsync(g => g.Id == command.GroupId && g.IsActive, cancellationToken);

            if (!groupExists)
                throw new FeatureException(
                    ModuleMarker.ModuleName,
                    "GROUP_NOT_FOUND",
                    $"Group with ID '{command.GroupId}' not found or inactive.",
                    "The specified group does not exist."
                );
        }

        if (command.ParentSkillId.HasValue)
        {
            var parentExists = await _context.Skills
                .AnyAsync(s => s.Id == command.ParentSkillId && !s.IsDeleted, cancellationToken);

            if (!parentExists)
                throw new FeatureException(
                    ModuleMarker.ModuleName,
                    "PARENT_SKILL_NOT_FOUND",
                    $"Parent skill with ID '{command.ParentSkillId}' not found.",
                    "The specified parent skill does not exist."
                );
        }

        var skill = Skill.Create(
            command.Name,
            command.Description,
            command.Origin,
            command.OrganizationId,
            command.CategoryId,
            command.GroupId,
            command.ParentSkillId,
            command.EstimatedTimeToLearn
        );

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync(cancellationToken);

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