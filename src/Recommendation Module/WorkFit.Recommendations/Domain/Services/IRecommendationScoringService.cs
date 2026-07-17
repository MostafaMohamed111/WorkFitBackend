using WorkFit.TalentManagement.Contracts.Dtos;

namespace WorkFit.Recommendations.Domain.Services;

public interface IRecommendationScoringService
{
    (decimal Score, string MatchReasoning) CalculateScore(
        IReadOnlyList<EmployeeSkillLookUpDto> employeeSkills,
        IReadOnlyList<Guid> requiredSkillIds);
}
