using Microsoft.EntityFrameworkCore;
using WorkFit.Skills.Domain.Entities;

namespace WorkFit.Skills.Infrastructure.Data;

public class WorkFitSkillsDbContext : DbContext
{
    public WorkFitSkillsDbContext(DbContextOptions<WorkFitSkillsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Skill> Skills { get; set; } = null!;
    public DbSet<SkillCategory> SkillCategories { get; set; } = null!;
    public DbSet<SkillGroup> SkillGroups { get; set; } = null!;
    public DbSet<SkillSynonym> SkillSynonyms { get; set; } = null!;
    public DbSet<SkillPrerequisite> SkillPrerequisites { get; set; } = null!;
    public DbSet<SkillRelation> SkillRelations { get; set; } = null!;
    public DbSet<EmergingSkill> EmergingSkills { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("Skills");

        // === Skill Self-Referencing (Parent-Child) ===
        modelBuilder.Entity<Skill>()
            .HasOne(s => s.ParentSkill)
            .WithMany(s => s.ChildSkills)
            .HasForeignKey(s => s.ParentSkillId)
            .OnDelete(DeleteBehavior.Restrict);

        // === Skill -> Category ===
        modelBuilder.Entity<Skill>()
            .HasOne(s => s.Category)
            .WithMany()
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // === Skill -> Group ===
        modelBuilder.Entity<Skill>()
            .HasOne(s => s.Group)
            .WithMany(g => g.Skills)
            .HasForeignKey(s => s.GroupId)
            .OnDelete(DeleteBehavior.SetNull);

        // === SkillPrerequisite: Skill -> Prerequisites ===
        modelBuilder.Entity<SkillPrerequisite>()
            .HasOne(sp => sp.Skill)
            .WithMany(s => s.Prerequisites)
            .HasForeignKey(sp => sp.SkillId)
            .OnDelete(DeleteBehavior.NoAction);

        // === SkillPrerequisite: Skill -> RequiredFor ===
        modelBuilder.Entity<SkillPrerequisite>()
            .HasOne(sp => sp.PrerequisiteSkill)
            .WithMany(s => s.RequiredFor)
            .HasForeignKey(sp => sp.PrerequisiteSkillId)
            .OnDelete(DeleteBehavior.NoAction);

        // === SkillRelation: Skill -> RelatedSkills ===
        modelBuilder.Entity<SkillRelation>()
            .HasOne(sr => sr.Skill)
            .WithMany(s => s.RelatedSkills)
            .HasForeignKey(sr => sr.SkillId)
            .OnDelete(DeleteBehavior.NoAction);

        // === SkillRelation: RelatedSkill -> RelatedFrom ===
        modelBuilder.Entity<SkillRelation>()
            .HasOne(sr => sr.RelatedSkill)
            .WithMany(s => s.RelatedFrom)
            .HasForeignKey(sr => sr.RelatedSkillId)
            .OnDelete(DeleteBehavior.NoAction);

        // === SkillSynonym ===
        modelBuilder.Entity<SkillSynonym>()
            .HasOne(ss => ss.Skill)
            .WithMany(s => s.Synonyms)
            .HasForeignKey(ss => ss.SkillId)
            .OnDelete(DeleteBehavior.Cascade);

        // === SkillGroup -> Category ===
        modelBuilder.Entity<SkillGroup>()
            .HasOne(sg => sg.Category)
            .WithMany(sc => sc.Groups)
            .HasForeignKey(sg => sg.CategoryId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}