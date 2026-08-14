using System.Text.Json;
using WorkFit.Rag.CrossCutting;
using WorkFit.Rag.Infrastructure.Qdrant;

namespace WorkFit.Rag.Tests;

public class QdrantPayloadReaderTests
{
    [Fact]
    public void Employee_ValidPayload_Deserializes()
    {
        var payload = JsonSerializer.SerializeToElement(new
        {
            employeeProfileId = Guid.NewGuid(),
            organizationId = Guid.NewGuid(),
            employeeName = "Jane",
            status = "Active",
            availableAllocation = 80,
            performanceScore = 0.9,
            skills = new[]
            {
                new { skillId = Guid.NewGuid(), name = "C#", level = 0.75 }
            }
        });

        var employee = QdrantPayloadReader.Employee(payload);

        Assert.NotNull(employee);
        Assert.Equal("Jane", employee!.EmployeeName);
        Assert.Single(employee.Skills!);
        Assert.Equal(0.75, employee.Skills![0].Level, precision: 10);
    }

    [Fact]
    public void Employee_TypeMismatchedPayload_ReturnsNull()
    {
        var payload = JsonSerializer.SerializeToElement(new { employeeProfileId = "not-a-guid" });

        Assert.Null(QdrantPayloadReader.Employee(payload));
    }

    [Fact]
    public void ProjectTask_ValidPayload_Deserializes()
    {
        var payload = JsonSerializer.SerializeToElement(new
        {
            taskId = Guid.NewGuid(),
            projectId = Guid.NewGuid(),
            organizationId = Guid.NewGuid(),
            employeeOutcomes = new[]
            {
                new { employeeProfileId = Guid.NewGuid(), performanceScore = 0.8 }
            }
        });

        var task = QdrantPayloadReader.ProjectTask(payload);

        Assert.NotNull(task);
        Assert.Single(task!.EmployeeOutcomes!);
        Assert.Equal(0.8, task.EmployeeOutcomes![0].PerformanceScore, precision: 10);
    }

    [Fact]
    public void ProjectTask_MissingOutcomes_ReturnsNullOutcomes()
    {
        var payload = JsonSerializer.SerializeToElement(new
        {
            taskId = Guid.NewGuid(),
            projectId = Guid.NewGuid(),
            organizationId = Guid.NewGuid()
        });

        var task = QdrantPayloadReader.ProjectTask(payload);

        Assert.NotNull(task);
        Assert.Null(task!.EmployeeOutcomes);
    }
}

public class IndexingSnapshotSanitizerTests
{
    [Theory]
    [InlineData(null, "fallback", "fallback")]
    [InlineData("", "fallback", "fallback")]
    [InlineData("   ", "fallback", "fallback")]
    [InlineData("value", "fallback", "value")]
    public void Required_EmptyOrWhitespace_UsesFallback(string? input, string fallback, string expected)
    {
        Assert.Equal(expected, IndexingSnapshotSanitizer.Required(input, fallback));
    }

    [Fact]
    public void Optional_NormalizesWhitespaceAndDropsControlCharacters()
    {
        Assert.Equal("Hello world", IndexingSnapshotSanitizer.Optional("Hello \t world"));
        Assert.Null(IndexingSnapshotSanitizer.Optional("   "));
        Assert.Null(IndexingSnapshotSanitizer.Optional(""));
        Assert.Null(IndexingSnapshotSanitizer.Optional(null));
        Assert.Equal("A B", IndexingSnapshotSanitizer.Optional("A\nB"));
    }
}
