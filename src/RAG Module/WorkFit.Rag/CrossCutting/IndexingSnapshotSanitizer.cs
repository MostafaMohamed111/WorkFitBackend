using System.Text;

namespace WorkFit.Rag.CrossCutting;

internal static class IndexingSnapshotSanitizer
{
    public static string Required(string? value, string fallback)
    {
        var sanitized = Optional(value);
        return string.IsNullOrEmpty(sanitized) ? fallback : sanitized;
    }

    public static string? Optional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var result = new StringBuilder(value.Length);
        var pendingSpace = false;
        foreach (var character in value)
        {
            if (char.IsControl(character) || char.IsWhiteSpace(character))
            {
                pendingSpace = result.Length > 0;
                continue;
            }

            if (pendingSpace)
            {
                result.Append(' ');
                pendingSpace = false;
            }

            result.Append(character);
        }

        return result.Length == 0 ? null : result.ToString();
    }
}
