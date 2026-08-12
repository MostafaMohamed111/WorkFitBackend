namespace WorkFit.Documents.Infrastructure.Configuration;

public sealed class LocalDocumentFileStorageOptions
{
    public const string SectionPath = "FileStorage:Local";

    /// <summary>
    /// Root directory for document blobs (mirrors blob key layout: yyyy/MM/dd/name.ext).
    /// When empty: Windows uses <c>C:\expatzi-storage\documents</c>; other OS uses <c>{ContentRootPath}/expatzi-storage/documents</c>.
    /// </summary>
    public string RootPath { get; set; } = string.Empty;
}
