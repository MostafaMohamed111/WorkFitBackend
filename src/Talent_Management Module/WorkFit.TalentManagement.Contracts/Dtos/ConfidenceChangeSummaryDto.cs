

namespace WorkFit.TalentManagement.Contracts.Dtos
{
    public sealed record ConfidenceChangeSummaryDto(
      Guid Id,
      Guid AssessmentId,
      int OldScore,
      int NewScore,
      DateTime CreatedAt);
}
