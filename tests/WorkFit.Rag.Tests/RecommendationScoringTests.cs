using WorkFit.Rag.Contracts.Recommendations;
using WorkFit.Rag.Domain;
using WorkFit.Rag.Infrastructure.Qdrant;

namespace WorkFit.Rag.Tests;

public class RecommendationScoringTests
{
    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(0.5, 0.75)]
    [InlineData(0.0, 0.5)]
    [InlineData(-1.0, 0.0)]
    [InlineData(1.5, 1.0)]
    public void NormalizeCosine_ProjectsCosineRangeToUnit(double cosine, double expected)
    {
        Assert.Equal(expected, RecommendationScoring.NormalizeCosine(cosine), precision: 10);
    }

    [Theory]
    [InlineData(double.NaN, 0)]
    [InlineData(double.PositiveInfinity, 0)]
    [InlineData(double.NegativeInfinity, 0)]
    public void Clamp_NonFiniteValues_ReturnZero(double value, double expected)
    {
        Assert.Equal(expected, RecommendationScoring.Clamp(value), precision: 10);
    }

    [Theory]
    [InlineData(-0.5, 0)]
    [InlineData(0.25, 0.25)]
    [InlineData(1.5, 1)]
    public void Clamp_BoundsValues(double value, double expected)
    {
        Assert.Equal(expected, RecommendationScoring.Clamp(value), precision: 10);
    }

    [Fact]
    public void CalculateSkillScore_PerfectMatch_ReturnsOne()
    {
        var required = new[]
        {
            new RequiredSkill(Guid.NewGuid(), "C#", 3, 1),
            new RequiredSkill(Guid.NewGuid(), "SQL", 2, 1)
        };
        var employee = new[]
        {
            new IndexedEmployeeSkill(required[0].SkillId, "C#", 5),
            new IndexedEmployeeSkill(required[1].SkillId, "SQL", 4)
        };

        var score = RecommendationScoring.CalculateSkillScore(required, employee, out var matched, out var missing);

        Assert.Equal(1, score, precision: 10);
        Assert.Equal(2, matched.Count);
        Assert.Empty(missing);
    }

    [Fact]
    public void CalculateSkillScore_LevelShortfall_RatiosProportionally()
    {
        var required = new[] { new RequiredSkill(Guid.NewGuid(), "React", 4, 1) };
        var employee = new[] { new IndexedEmployeeSkill(required[0].SkillId, "React", 2) };

        var score = RecommendationScoring.CalculateSkillScore(required, employee, out var matched, out var missing);

        Assert.Equal(0.5, score, precision: 10);
        Assert.Single(matched);
        Assert.Empty(missing);
    }

    [Fact]
    public void CalculateSkillScore_MissingSkill_ExcludedFromNumerator()
    {
        var required = new[]
        {
            new RequiredSkill(Guid.NewGuid(), "Docker", 2, 1),
            new RequiredSkill(Guid.NewGuid(), "Kubernetes", 3, 1)
        };
        var employee = new[] { new IndexedEmployeeSkill(required[0].SkillId, "Docker", 4) };

        var score = RecommendationScoring.CalculateSkillScore(required, employee, out var matched, out var missing);

        Assert.Equal(0.5, score, precision: 10);
        Assert.Single(matched);
        Assert.Single(missing);
        Assert.Contains("Kubernetes", missing);
    }

    [Fact]
    public void CalculateSkillScore_MatchesByNameWhenIdMismatched()
    {
        var required = new[] { new RequiredSkill(Guid.NewGuid(), "Python", 3, 1) };
        var employee = new[] { new IndexedEmployeeSkill(Guid.NewGuid(), "python", 3) };

        var score = RecommendationScoring.CalculateSkillScore(required, employee, out var matched, out var missing);

        Assert.Equal(1, score, precision: 10);
        Assert.Single(matched);
        Assert.Empty(missing);
    }

    [Fact]
    public void CalculateSkillScore_ZeroTotalWeight_ReturnsOne()
    {
        var required = new[] { new RequiredSkill(Guid.NewGuid(), "Any", 1, 0) };
        var employee = new[] { new IndexedEmployeeSkill(Guid.NewGuid(), "Any", 1) };

        var score = RecommendationScoring.CalculateSkillScore(required, employee, out _, out _);

        Assert.Equal(1, score, precision: 10);
    }

    [Fact]
    public void CalculateSkillScore_NegativeWeight_IgnoredFromTotal()
    {
        var required = new[]
        {
            new RequiredSkill(Guid.NewGuid(), "A", 1, -1),
            new RequiredSkill(Guid.NewGuid(), "B", 1, 1)
        };
        var employee = new[] { new IndexedEmployeeSkill(required[0].SkillId, "A", 1) };

        var score = RecommendationScoring.CalculateSkillScore(required, employee, out _, out _);

        Assert.Equal(0, score, precision: 10);
    }

    [Fact]
    public void WeightedScore_UsesConfiguredWeights()
    {
        var score = RecommendationScoring.WeightedScore(
            0.5, 0.5, 0.5, 0.5,
            semanticWeight: 0.35, skillWeight: 0.30, performanceWeight: 0.20, llmWeight: 0.15);

        Assert.Equal(0.5, score, precision: 10);
    }

    [Fact]
    public void WeightedScore_ZeroTotalWeight_ReturnsZero()
    {
        var score = RecommendationScoring.WeightedScore(0.5, 0.5, 0.5, 0.5, 0, 0, 0, 0);

        Assert.Equal(0, score, precision: 10);
    }

    [Fact]
    public void WeightedScore_OutOfRangeInputs_AreClampedIndividually()
    {
        var score = RecommendationScoring.WeightedScore(2, -1, 0.25, 0.25, 0.25, 0.25, 0.25, 0.25);

        Assert.Equal(0.375, score, precision: 10);
    }

    [Fact]
    public void WeightedScore_NegativeWeights_TreatedAsZero()
    {
        var score = RecommendationScoring.WeightedScore(1, 0, 0, 0, -1, 0, 0, 0);

        Assert.Equal(0, score, precision: 10);
    }
}
