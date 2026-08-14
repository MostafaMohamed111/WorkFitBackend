using System.Text.Json;

namespace WorkFit.Rag.Infrastructure.Qdrant;

internal static class QdrantPayloadReader
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static IndexedEmployeeProfile? Employee(JsonElement payload)
    {
        try
        {
            return payload.Deserialize<IndexedEmployeeProfile>(Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static IndexedProjectTask? ProjectTask(JsonElement payload)
    {
        try
        {
            return payload.Deserialize<IndexedProjectTask>(Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
