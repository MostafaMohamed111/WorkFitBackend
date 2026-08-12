using WorkFit.Rag.Infrastructure.Qdrant;

namespace WorkFit.Rag.Tests;

public class DeterministicPointIdTests
{
    [Fact]
    public void EmployeeProfile_SameInput_ReturnsSameId()
    {
        var id = Guid.NewGuid();

        Assert.Equal(
            DeterministicPointId.EmployeeProfile(id),
            DeterministicPointId.EmployeeProfile(id));
    }

    [Fact]
    public void ProjectTask_SameInput_ReturnsSameId()
    {
        var id = Guid.NewGuid();

        Assert.Equal(
            DeterministicPointId.ProjectTask(id),
            DeterministicPointId.ProjectTask(id));
    }

    [Fact]
    public void EmployeeProfile_DifferentInput_ReturnsDifferentId()
    {
        Assert.NotEqual(
            DeterministicPointId.EmployeeProfile(Guid.NewGuid()),
            DeterministicPointId.EmployeeProfile(Guid.NewGuid()));
    }

    [Fact]
    public void EmployeeProfile_And_ProjectTask_UseDifferentNamespaces()
    {
        var id = Guid.NewGuid();

        Assert.NotEqual(
            DeterministicPointId.EmployeeProfile(id),
            DeterministicPointId.ProjectTask(id));
    }

    [Fact]
    public void EmployeeProfile_ProducesVersion5Uuid()
    {
        var bytes = DeterministicPointId.EmployeeProfile(Guid.NewGuid()).ToByteArray(bigEndian: true);

        Assert.Equal(0x50, bytes[6] & 0xf0);
        Assert.Equal(0x80, bytes[8] & 0xc0);
    }

    [Fact]
    public void ProjectTask_ProducesVersion5Uuid()
    {
        var bytes = DeterministicPointId.ProjectTask(Guid.NewGuid()).ToByteArray(bigEndian: true);

        Assert.Equal(0x50, bytes[6] & 0xf0);
        Assert.Equal(0x80, bytes[8] & 0xc0);
    }
}
