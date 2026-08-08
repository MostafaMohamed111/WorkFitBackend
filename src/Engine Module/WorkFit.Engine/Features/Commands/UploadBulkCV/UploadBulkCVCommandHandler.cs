using System.IO.Compression;
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

namespace WorkFit.Engine.Features.Commands.UploadBulkCV;

internal sealed class UploadBulkCVCommandHandler : IRequestHandler<UploadBulkCVCommand, UploadCVBulkResponse>
{
    // Path B (Identity): anonymous bulk CV upload, see UploadSingleCVCommandHandler.
    private static readonly Guid PendingOrganizationId = Guid.Empty;

    private readonly EngineDbContext _db;
    private readonly CVTextExtractorAggregator _extractor;
    private readonly CVProcessingChannel _channel;
    private readonly IOptions<CVParsingOptions> _options;

    public UploadBulkCVCommandHandler(
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

    public async Task<UploadCVBulkResponse> Handle(UploadBulkCVCommand command, CancellationToken cancellationToken = default)
    {
        var zipFile = command.Zip;
        if (zipFile is null || zipFile.Length == 0)
            throw new FeatureException(ModuleMarker.ModuleName, "MISSING_ZIP", "No zip file was uploaded.", "Please upload a zip file.");

        var maxBatchBytes = _options.Value.MaxBatchMb * 1024L * 1024L;
        if (zipFile.Length > maxBatchBytes)
            throw new FeatureException(ModuleMarker.ModuleName, "ZIP_TOO_LARGE", $"Zip exceeds {_options.Value.MaxBatchMb}MB limit.", "The uploaded zip is too large.");

        var batchId = Guid.NewGuid();
        var jobs = new List<UploadCVResponse>();

        await using var zipStream = zipFile.OpenReadStream();
        using var zip = new ZipArchive(zipStream, ZipArchiveMode.Read);
        if (zip.Entries.Count > _options.Value.MaxBatchFiles)
            throw new FeatureException(ModuleMarker.ModuleName, "ZIP_TOO_MANY_FILES", $"Zip contains too many entries (max {_options.Value.MaxBatchFiles}).", $"Zip can contain at most {_options.Value.MaxBatchFiles} files.");

        foreach (var entry in zip.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entry.Length == 0) continue;

            var ext = Path.GetExtension(entry.Name);
            if (!ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase) && !ext.Equals(".docx", StringComparison.OrdinalIgnoreCase))
                continue;

            await using var entryStream = entry.Open();
            using var memStream = new MemoryStream();
            await entryStream.CopyToAsync(memStream, cancellationToken);
            memStream.Position = 0;

            string hash;
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = await sha.ComputeHashAsync(memStream, cancellationToken);
                hash = Convert.ToHexString(hashBytes);
            }
            memStream.Position = 0;

            var existing = await _db.CVParseJobs
                .Where(j => j.OrganizationId == PendingOrganizationId && j.FileHash == hash && j.Status == "Succeeded"
                            && j.EmployeeProfileId != null)
                .FirstOrDefaultAsync(cancellationToken);

            if (existing is not null)
            {
                jobs.Add(new UploadCVResponse(existing.Id, CVParseJobStatus.SkippedDuplicate, "Skipped duplicate."));
                continue;
            }

            var mime = ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase)
                ? "application/pdf"
                : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            var extraction = await _extractor.ExtractAsync(entry.Name, mime, memStream, cancellationToken);

            var documentId = Guid.NewGuid().ToString("N");
            var job = CVParseJob.Create(PendingOrganizationId, batchId, documentId, entry.Name, mime, hash);
            if (extraction.Success) job.SetExtractedText(extraction.Text);
            else job.SetFailed("FailedExtraction", extraction.Error ?? "Extraction failed.");
            _db.CVParseJobs.Add(job);
            await _db.SaveChangesAsync(cancellationToken);

            if (extraction.Success)
            {
                await _channel.EnqueueAsync(new ProcessCVJobMessage(job.Id), cancellationToken);
                jobs.Add(new UploadCVResponse(job.Id, Enum.Parse<CVParseJobStatus>(job.Status)));
            }
            else
            {
                jobs.Add(new UploadCVResponse(job.Id, CVParseJobStatus.FailedExtraction, extraction.Error));
            }
        }

        return new UploadCVBulkResponse(batchId, jobs.Count, jobs.Count, jobs);
    }
}