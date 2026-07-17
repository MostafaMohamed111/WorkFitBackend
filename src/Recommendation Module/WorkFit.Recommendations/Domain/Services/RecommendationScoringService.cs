using System.Text;
using WorkFit.TalentManagement.Contracts.Dtos;

namespace WorkFit.Recommendations.Domain.Services;

public sealed class RecommendationScoringService : IRecommendationScoringService
{
    public (decimal Score, string MatchReasoning) CalculateScore(
        IReadOnlyList<EmployeeSkillLookUpDto> employeeSkills,
        IReadOnlyList<Guid> requiredSkillIds)
    {
        if (requiredSkillIds == null || requiredSkillIds.Count == 0)
        {
            return (0m, "No skills required for this project.");
        }

        var matchedSkills = employeeSkills
            .Where(s => requiredSkillIds.Contains(s.SkillId))
            .ToList();

        if (matchedSkills.Count == 0)
        {
            return (0m, "Candidate has none of the required skills.");
        }

        // Basic scoring: percentage of required skills matched.
        // We can factor in ConfidenceScore to give higher rank to highly skilled employees.
        decimal baseMatchPercentage = (decimal)matchedSkills.Count / requiredSkillIds.Count;
        
        // Average confidence of the matched skills (0 to 100)
        decimal averageConfidence = (decimal)matchedSkills.Average(s => s.ConfidenceScore);
        
        // Final score out of 100: 70% weight to skill match count, 30% weight to confidence level.
        decimal finalScore = (baseMatchPercentage * 70m) + (averageConfidence * 0.3m);

        var reasoningBuilder = new StringBuilder();
        reasoningBuilder.Append($"Matched {matchedSkills.Count} of {requiredSkillIds.Count} required skills. ");
        reasoningBuilder.Append("Matched Skills: ");
        reasoningBuilder.Append(string.Join(", ", matchedSkills.Select(s => $"{s.SkillName} (Confidence: {s.ConfidenceScore}%)")));

        return (Math.Round(finalScore, 2), reasoningBuilder.ToString());
    }
}
