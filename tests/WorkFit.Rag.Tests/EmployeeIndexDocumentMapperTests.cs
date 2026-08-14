using WorkFit.Rag.CrossCutting;
using WorkFit.TalentManagement.Contracts.Indexing;

namespace WorkFit.Rag.Tests;

public class EmployeeIndexDocumentMapperTests
{
    private static EmployeeIndexingSnapshot Employee(Action<EmployeeIndexingSnapshotBuilder>? configure = null)
    {
        var builder = new EmployeeIndexingSnapshotBuilder();
        configure?.Invoke(builder);
        return builder.Build();
    }

    [Fact]
    public void Map_SanitizesBlankNameAndStatus()
    {
        var snapshot = Employee(builder =>
        {
            builder.Name = "   ";
            builder.Status = "";
        });

        var document = EmployeeIndexDocumentMapper.Map(snapshot);

        Assert.Equal("Unnamed employee", document.EmployeeName);
        Assert.Equal("Unknown", document.Status);
    }

    [Fact]
    public void Map_AvailableAllocation_IsRemainingCapacity()
    {
        var snapshot = Employee(builder => builder.CurrentAllocationPercentage = 40);

        var document = EmployeeIndexDocumentMapper.Map(snapshot);

        Assert.Equal(60, document.AvailableAllocation, precision: 10);
    }

    [Fact]
    public void Map_CurrentAllocationOverHundred_ClampsToZeroAvailable()
    {
        var snapshot = Employee(builder => builder.CurrentAllocationPercentage = 150);

        var document = EmployeeIndexDocumentMapper.Map(snapshot);

        Assert.Equal(0, document.AvailableAllocation, precision: 10);
    }

    [Fact]
    public void Map_SkillConfidenceNormalizedToUnitInterval()
    {
        var snapshot = Employee(builder => builder.Skills = new[]
        {
            new EmployeeSkillIndexingSnapshot(Guid.NewGuid(), "C#", 75, Array.Empty<EmployeeSkillEvidenceIndexingSnapshot>())
        });

        var document = EmployeeIndexDocumentMapper.Map(snapshot);

        Assert.Single(document.Skills);
        Assert.Equal(0.75, document.Skills[0].Level, precision: 10);
    }

    [Fact]
    public void Map_BlankSkillNamesAreExcluded()
    {
        var snapshot = Employee(builder => builder.Skills = new[]
        {
            new EmployeeSkillIndexingSnapshot(Guid.NewGuid(), "   ", 50, Array.Empty<EmployeeSkillEvidenceIndexingSnapshot>())
        });

        var document = EmployeeIndexDocumentMapper.Map(snapshot);

        Assert.Empty(document.Skills);
    }

    [Fact]
    public void Map_PerformanceWithoutEvidence_ReturnsZero()
    {
        var snapshot = Employee(builder => builder.TaskPerformance = null);

        var document = EmployeeIndexDocumentMapper.Map(snapshot);

        Assert.Equal(0, document.PerformanceScore, precision: 10);
    }

    [Fact]
    public void Map_PerformanceWithZeroAssigned_ReturnsZero()
    {
        var snapshot = Employee(builder => builder.TaskPerformance =
            new EmployeeTaskPerformanceIndexingSnapshot(0, 0, 0, null));

        var document = EmployeeIndexDocumentMapper.Map(snapshot);

        Assert.Equal(0, document.PerformanceScore, precision: 10);
    }

    [Fact]
    public void Map_PerformanceBlendsCompletionRatioAndStoryPointEvidence()
    {
        var snapshot = Employee(builder => builder.TaskPerformance =
            new EmployeeTaskPerformanceIndexingSnapshot(AssignedTaskCount: 4, CompletedTaskCount: 4, CompletedStoryPoints: 20, null));

        var document = EmployeeIndexDocumentMapper.Map(snapshot);

        Assert.Equal(1.0, document.PerformanceScore, precision: 10);
    }

    [Fact]
    public void Map_ProfileSummaryIncludesJobTitleBioCertificationsAndEvidence()
    {
        var snapshot = Employee(builder =>
        {
            builder.JobTitle = "Senior Engineer";
            builder.Bio = "   Leads   teams ";
            builder.Certifications = new[]
            {
                new EmployeeCertificationIndexingSnapshot("AWS SA", "Amazon", new DateOnly(2024, 1, 1), new DateOnly(2027, 1, 1), false)
            };
            builder.Skills = new[]
            {
                new EmployeeSkillIndexingSnapshot(Guid.NewGuid(), "C#", 80, new[]
                {
                    new EmployeeSkillEvidenceIndexingSnapshot("Assessment", "Proficient", new DateTime(2025, 1, 1))
                })
            };
        });

        var document = EmployeeIndexDocumentMapper.Map(snapshot);

        Assert.Contains("Job title: Senior Engineer.", document.ProfileSummary);
        Assert.Contains("Bio: Leads teams.", document.ProfileSummary);
        Assert.Contains("Certifications: AWS SA from Amazon.", document.ProfileSummary);
        Assert.Contains("Skill evidence: C#: Assessment: Proficient.", document.ProfileSummary);
    }

    [Fact]
    public void Map_ExpiredCertificationsAreOmittedFromSummary()
    {
        var snapshot = Employee(builder => builder.Certifications = new[]
        {
            new EmployeeCertificationIndexingSnapshot("Old Cert", "Issuer", new DateOnly(2020, 1, 1), new DateOnly(2021, 1, 1), true)
        });

        var document = EmployeeIndexDocumentMapper.Map(snapshot);

        Assert.DoesNotContain("Old Cert", document.ProfileSummary);
    }

    [Fact]
    public void IsRemoved_StatusInactiveOrDeleted_ReturnsTrue()
    {
        var inactive = Employee(builder => builder.Status = "Inactive");
        var deleted = Employee(builder => builder.Status = "Deleted");

        Assert.True(EmployeeIndexDocumentMapper.IsRemoved(inactive));
        Assert.True(EmployeeIndexDocumentMapper.IsRemoved(deleted));
    }

    [Fact]
    public void IsRemoved_ActiveStatus_ReturnsFalse()
    {
        var active = Employee(builder => builder.Status = "Active");

        Assert.False(EmployeeIndexDocumentMapper.IsRemoved(active));
    }

    [Theory]
    [InlineData("Deactivated")]
    [InlineData("Deleted")]
    public void IsRemoved_DeletionChangeTypes_ReturnTrue(string changeType)
    {
        var active = Employee(builder => builder.Status = "Active");

        Assert.True(EmployeeIndexDocumentMapper.IsRemoved(active, changeType));
    }

    [Fact]
    public void IsRemoved_UpdateChangeType_ReturnsFalse()
    {
        var active = Employee(builder => builder.Status = "Active");

        Assert.False(EmployeeIndexDocumentMapper.IsRemoved(active, "Updated"));
    }

    [Fact]
    public void Map_NullSnapshot_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => EmployeeIndexDocumentMapper.Map(null!));
    }
}

internal sealed class EmployeeIndexingSnapshotBuilder
{
    public string Name { get; set; } = "Jane Doe";
    public string JobTitle { get; set; } = "Engineer";
    public string? Bio { get; set; }
    public string Status { get; set; } = "Active";
    public int CurrentAllocationPercentage { get; set; }
    public IReadOnlyList<EmployeeSkillIndexingSnapshot> Skills { get; set; } =
        Array.Empty<EmployeeSkillIndexingSnapshot>();
    public IReadOnlyList<EmployeeCertificationIndexingSnapshot> Certifications { get; set; } =
        Array.Empty<EmployeeCertificationIndexingSnapshot>();
    public EmployeeTaskPerformanceIndexingSnapshot? TaskPerformance { get; set; }

    public EmployeeIndexingSnapshot Build() => new(
        Guid.NewGuid(), Guid.NewGuid(), Name, JobTitle, Bio, Status, CurrentAllocationPercentage,
        null, Skills, Certifications, TaskPerformance, DateTimeOffset.UtcNow);
}
