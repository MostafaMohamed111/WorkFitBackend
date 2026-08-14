namespace WorkFit.WorkFlow.Features.UploadCvs;

public sealed partial class UploadCvsCommandHandler
{
    // A CV pending storage, regardless of whether it arrived as a loose file or a ZIP entry.
    private sealed record CvCandidate(string FileName, string ContentType, Stream Content, long Size);
}
