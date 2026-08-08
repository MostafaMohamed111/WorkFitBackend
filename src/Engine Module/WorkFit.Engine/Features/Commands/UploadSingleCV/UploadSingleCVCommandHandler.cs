using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.Engine.Domain.Entities;
using WorkFit.Engine.Infrastructure.CVParsing;
using WorkFit.Engine.Infrastructure.Data;
using WorkFit.Engine.Infrastructure.Extraction;
using WorkFit.Engine.Infrastructure.Options;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Features.Commands.UploadSingleCV;

internal sealed class UploadSingleCVCommandHandler : IRequestHandler<UploadSingleCVCommand, UploadCVResponse>
{
    // Path B (Identity): anonymous CV upload. No authenticated user yet, so the
    // organization is left empty and assigned by HR during the review flow.
    private static readonly Guid PendingOrganizationId = Guid.Empty;

    private readonly EngineDbContext _db;
    private readonly CVTextExtractorAggregator _extractor;
    private readonly CVProcessingChannel _channel;
    private readonly IOptions<CVParsingOptions> _options;

    public UploadSingleCVCommandHandler(
        EngineDbContext db,
        CVTextExtractorAggregator extractor,
        CVProcessingChannel channel,
        IOptions<CVParsingOptions> options)
    {
        _db = db;
        _extractor = extractor;
        _channel = channel;
        _options = options;
    }

    public async Task<UploadCVResponse> Handle(UploadSingleCVCommand command, CancellationToken cancellationToken = default)
    {
        var file = command.File;
        if (file is null || file.Length == 0)
            throw new FeatureException(ModuleMarker.ModuleName, "MISSING_FILE", "No file was uploaded.", "Please upload a CV file.");

        var maxBytes = _options.Value.MaxFileMb * 1024L * 1024L;
        if (file.Length > maxBytes)
            throw new FeatureException(ModuleMarker.ModuleName, "FILE_TOO_LARGE", $"File exceeds {_options.Value.MaxFileMb}MB limit.", "The uploaded CV is too large.");

        await using var stream = file.OpenReadStream();
        string hash;
        using (var sha = System.Security.Cryptography.SHA256.Create())
        {
            var hashBytes = await sha.ComputeHashAsync(stream, cancellationToken);
            hash = Convert.ToHexString(hashBytes);
        }
        stream.Position = 0;

        var existing = await _db.CVParseJobs
            .Where(j => j.OrganizationId == PendingOrganizationId && j.FileHash == hash && j.Status == "Succeeded"
                        && j.EmployeeProfileId != null)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is not null)
            return new UploadCVResponse(existing.Id, CVParseJobStatus.SkippedDuplicate, "Skipped: this CV was already parsed.");

        CVExtractionResult extraction;
        await using (var memStream = new MemoryStream())
        {
            await stream.CopyToAsync(memStream, cancellationToken);
            memStream.Position = 0;
            extraction = await _extractor.ExtractAsync(file.FileName, file.ContentType, memStream, cancellationToken);
        }

        var documentId = Guid.NewGuid().ToString("N");
        var job = CVParseJob.Create(PendingOrganizationId, batchId: null, documentId, file.FileName, file.ContentType, hash);
        if (extraction.Success) job.SetExtractedText(extraction.Text);
        _db.CVParseJobs.Add(job);
        await _db.SaveChangesAsync(cancellationToken);

        if (!extraction.Success)
        {
            job.SetFailed("FailedExtraction", extraction.Error ?? "Extraction failed.");
            await _db.SaveChangesAsync(cancellationToken);
            return new UploadCVResponse(job.Id, CVParseJobStatus.FailedExtraction, extraction.Error);
        }

        await _channel.EnqueueAsync(new ProcessCVJobMessage(job.Id), cancellationToken);
        var status = Enum.Parse<CVParseJobStatus>(job.Status);
        return new UploadCVResponse(job.Id, status);
    }
}