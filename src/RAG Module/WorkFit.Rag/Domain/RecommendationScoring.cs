using WorkFit.Rag.Contracts.Recommendations;
using WorkFit.Rag.Infrastructure.Qdrant;

namespace WorkFit.Rag.Domain;

internal static class RecommendationScoring
{
    public static double NormalizeCosine(double score) => Clamp((score + 1) / 2);

    public static double CalculateSkillScore(
        IReadOnlyList<RequiredSkill> requiredSkills,
        IReadOnlyList<IndexedEmployeeSkill> employeeSkills,
        out IReadOnlyList<string> matched,
        out IReadOnlyList<string> missing)
    {
        var matchedSkills = new List<string>();
        var missingSkills = new List<string>();
        var weightedScore = 0d;
        var totalWeight = 0d;

        foreach (var required in requiredSkills)
        {
            var weight = Math.Max(0, required.Weight);
            totalWeight += weight;
            var employeeSkill = employeeSkills.FirstOrDefault(skill =>
                required.SkillId.HasValue && skill.SkillId == required.SkillId ||
                string.Equals(skill.Name, required.Name, StringComparison.OrdinalIgnoreCase));

            if (employeeSkill is null)
            {
                missingSkills.Add(required.Name);
                continue;
            }

            matchedSkills.Add(required.Name);
            var requiredLevel = required.RequiredLevel <= 0 ? 1 : required.RequiredLevel;
            weightedScore += Clamp(employeeSkill.Level / requiredLevel) * weight;
        }

        matched = matchedSkills;
        missing = missingSkills;
        return totalWeight <= 0 ? 1 : Clamp(weightedScore / totalWeight);
    }

    public static double WeightedScore(
        double semantic,
        double skill,
        double performance,
        double llm,
        double semanticWeight,
        double skillWeight,
        double performanceWeight,
        double llmWeight)
    {
        var weights = new[]
        {
            Math.Max(0, semanticWeight), Math.Max(0, skillWeight),
            Math.Max(0, performanceWeight), Math.Max(0, llmWeight)
        };
        var total = weights.Sum();
        if (total <= 0)
        {
            return 0;
        }

        return Clamp((Clamp(semantic) * weights[0] + Clamp(skill) * weights[1] +
            Clamp(performance) * weights[2] + Clamp(llm) * weights[3]) / total);
    }

    public static double Clamp(double value) => double.IsFinite(value) ? Math.Clamp(value, 0, 1) : 0;
}
