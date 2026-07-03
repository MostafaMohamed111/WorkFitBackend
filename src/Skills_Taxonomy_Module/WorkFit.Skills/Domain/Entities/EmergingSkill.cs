using WorkFit.SharedKernel;
using WorkFit.SharedKernel.BaseEntity;
using WorkFit.SharedKernel.Exceptions.DomainExceptions;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.Skills.Domain.Enums;

namespace WorkFit.Skills.Domain.Entities;

public sealed class EmergingSkill : BaseEntity
{
    // === Properties ===
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }
    public string? Description { get; private set; }
    public string SuggestedCategory { get; private set; }
    public double ConfidenceScore { get; private set; }
    public string SourceEvidenceJson { get; private set; }
    public int OccurrenceCount { get; private set; }
    public EmergingSkillStatus Status { get; private set; }
    public string? SuggestedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public Guid? ApprovedSkillId { get; private set; }

    // === Private Constructor (EF Core) ===
    private EmergingSkill() { }

    // === Factory Method ===
    public static EmergingSkill Create(
        string name,
        string suggestedCategory,
        double confidenceScore,
        string sourceEvidenceJson,
        int occurrenceCount = 1,
        string? suggestedBy = "AI")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new FeildIsNullOrEmptyException(
                ModuleMarker.ModuleName,
                nameof(EmergingSkill),
                nameof(name)
            );

        if (confidenceScore < 0 || confidenceScore > 1)
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "INVALID_CONFIDENCE",
                "Confidence score must be between 0 and 1.",
                "Invalid confidence score provided."
            );

        return new EmergingSkill
        {
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
            SuggestedCategory = suggestedCategory,
            ConfidenceScore = confidenceScore,
            SourceEvidenceJson = sourceEvidenceJson,
            OccurrenceCount = occurrenceCount,
            Status = EmergingSkillStatus.Pending,
            SuggestedBy = suggestedBy,
        };
    }

    // === Domain Methods ===
    public void Approve(Guid approvedSkillId)
    {
        if (Status != EmergingSkillStatus.Pending)
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "WRONG_EMERGING_STATUS",
                $"Cannot approve: status is {Status}",
                $"Emerging skill is not in pending state."
            );

        Status = EmergingSkillStatus.Approved;
        ApprovedSkillId = approvedSkillId;
        ApprovedAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public void Reject(string reason)
    {
        if (Status != EmergingSkillStatus.Pending)
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "WRONG_EMERGING_STATUS",
                $"Cannot reject: status is {Status}",
                $"Emerging skill is not in pending state."
            );

        Status = EmergingSkillStatus.Rejected;
        RejectionReason = reason;
        RejectedAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public void IncrementOccurrence()
    {
        OccurrenceCount++;
        MarkUpdated();
    }

    public void UpdateConfidence(double newConfidence)
    {
        if (newConfidence < 0 || newConfidence > 1)
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "INVALID_CONFIDENCE",
                "Confidence score must be between 0 and 1.",
                "Invalid confidence score provided."
            );

        ConfidenceScore = newConfidence;
        MarkUpdated();
    }
}