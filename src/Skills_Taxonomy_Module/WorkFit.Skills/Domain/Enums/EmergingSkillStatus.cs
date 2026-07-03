namespace WorkFit.Skills.Domain.Enums;

public enum EmergingSkillStatus
{
    Pending = 0,     // Awaiting review
    Approved = 1,    // Approved -> will become a real Skill
    Rejected = 2     // Rejected, won't become a skill
}