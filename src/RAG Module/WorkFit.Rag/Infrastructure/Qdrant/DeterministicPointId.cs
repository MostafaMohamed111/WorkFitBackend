using System.Security.Cryptography;
using System.Text;

namespace WorkFit.Rag.Infrastructure.Qdrant;

internal static class DeterministicPointId
{
    private static readonly Guid NamespaceId = new("f3282933-19b7-5bce-b499-172f199babf4");

    public static Guid EmployeeProfile(Guid employeeProfileId) => Create($"employee:{employeeProfileId:N}");

    public static Guid ProjectTask(Guid taskId) => Create($"task:{taskId:N}");

    private static Guid Create(string name)
    {
        Span<byte> namespaceBytes = stackalloc byte[16];
        NamespaceId.TryWriteBytes(namespaceBytes, bigEndian: true, out _);
        var nameBytes = Encoding.UTF8.GetBytes(name);
        var input = new byte[namespaceBytes.Length + nameBytes.Length];
        namespaceBytes.CopyTo(input);
        nameBytes.CopyTo(input.AsSpan(namespaceBytes.Length));

        Span<byte> hash = stackalloc byte[20];
        SHA1.HashData(input, hash);
        hash[6] = (byte)((hash[6] & 0x0f) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3f) | 0x80);
        return new Guid(hash[..16], bigEndian: true);
    }
}
