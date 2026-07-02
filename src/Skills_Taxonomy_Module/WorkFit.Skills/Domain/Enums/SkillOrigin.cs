namespace WorkFit.Skills.Domain.Enums;

public enum SkillOrigin
{
    System = 0,      // Platform-wide, read-only, created by SuperAdmin
    Custom = 1,      // Organization-specific, created by Organization Admin
    Emerging = 2     // Created from approved emerging skill
}