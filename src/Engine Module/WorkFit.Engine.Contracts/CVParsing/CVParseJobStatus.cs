namespace WorkFit.Engine.Contracts.CVParsing;

public enum CVParseJobStatus
{
    Queued = 0,
    Processing = 1,
    Succeeded = 2,
    FailedExtraction = 3,
    FailedLLM = 4,
    InvalidDocument = 5,
    FailedTransient = 6,
    SkippedDuplicate = 7
}
