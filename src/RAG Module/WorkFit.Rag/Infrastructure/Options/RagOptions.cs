namespace WorkFit.Rag.Infrastructure.Options;

public sealed class QdrantOptions
{
    public const string SectionName = "Rag:Qdrant";
    public const int VectorSize = 1024;

    public string Url { get; set; } = "http://localhost:6333";
    public string? ApiKey { get; set; }
    public string EmployeeProfilesCollection { get; set; } = "employee_profiles";
    public string ProjectTasksCollection { get; set; } = "project_tasks";
    public int TimeoutSeconds { get; set; } = 30;
}

public sealed class RagRecommendationOptions
{
    public const string SectionName = "Rag:Recommendation";

    public string EmbeddingModel { get; set; } = "gemini-embedding-2";
    public string ChatModel { get; set; } = "mistral-small-latest";
    public int RetrievalLimit { get; set; } = 30;
    public int HistoricalTaskLimit { get; set; } = 20;
    public int ChatMaxTokens { get; set; } = 2000;
    public double SemanticWeight { get; set; } = 0.35;
    public double SkillWeight { get; set; } = 0.30;
    public double PerformanceWeight { get; set; } = 0.20;
    public double LlmWeight { get; set; } = 0.15;
}
