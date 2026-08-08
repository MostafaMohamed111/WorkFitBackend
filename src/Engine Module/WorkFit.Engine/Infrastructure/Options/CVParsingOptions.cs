namespace WorkFit.Engine.Infrastructure.Options;

public sealed class CVParsingOptions
{
    public int MaxFileMb { get; init; } = 10;
    public int MaxBatchMb { get; init; } = 500;
    public int MaxBatchFiles { get; init; } = 500;
    public int Concurrency { get; init; } = 3;
    public int ChannelCapacity { get; init; } = 100;
    public int HeartbeatTimeoutSeconds { get; init; } = 90;
    public int TextRetentionDays { get; init; } = 90;
}
