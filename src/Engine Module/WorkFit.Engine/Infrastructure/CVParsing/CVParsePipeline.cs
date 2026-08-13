using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.Engine.Contracts.CVParsing.IntegrationEvents;
using WorkFit.Engine.Domain.Entities;
using WorkFit.Engine.Infrastructure.Data;
using WorkFit.Engine.Infrastructure.Extraction;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Contracts.WriteServices.CreateEmployee;

namespace WorkFit.Engine.Infrastructure.CVParsing;

public interface ICVParsePipeline
{
    Task ExecuteAsync(Guid jobId, CancellationToken ct = default);
}

public sealed class CVParsePipeline : ICVParsePipeline
{
    private const string ExternalSourceSystem = "CV";
    private static readonly Guid PendingUserId = Guid.Empty;

    private readonly EngineDbContext _db;
    private readonly CVTextExtractorAggregator _extractor;
    private readonly ICVLLMParser _parser;
    private readonly ICVSkillNormalizer _skillNormalizer;
    private readonly IGetOrCreateExternalEmployeeService _getOrCreateEmployee;
    private readonly IMediator _mediator;
    private readonly ILogger<CVParsePipeline> _logger;
    private readonly TimeProvider _clock = TimeProvider.System;

    public CVParsePipeline(
        EngineDbContext db,
        CVTextExtractorAggregator extractor,
        ICVLLMParser parser,
        ICVSkillNormalizer skillNormalizer,
        IGetOrCreateExternalEmployeeService getOrCreateEmployee,
        IMediator mediator,
        ILogger<CVParsePipeline> logger)
    {
        _db = db;
        _extractor = extractor;
        _parser = parser;
        _skillNormalizer = skillNormalizer;
        _getOrCreateEmployee = getOrCreateEmployee;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _db.CVParseJobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job is null) { _logger.LogWarning("CVParseJob {JobId} not found.", jobId); return; }

        // Skip if already done (idempotent — re-run from crash).
        if (job.Status is "Succeeded" or "SkippedDuplicate" or "InvalidDocument" or "FailedExtraction") return;

        // Dedup check across all Succeeded jobs in same org with same hash.
        var existingSuccess = await _db.CVParseJobs
            .Where(j => j.OrganizationId == job.OrganizationId && j.FileHash == job.FileHash
                        && j.Status == "Succeeded" && j.Id != job.Id && j.EmployeeProfileId != null)
            .FirstOrDefaultAsync(ct);
        if (existingSuccess is not null)
        {
            job.SetSkipped(existingSuccess.EmployeeProfileId!.Value);
            await _db.SaveChangesAsync(ct);
            return;
        }

        job.SetProcessing();
        await _db.SaveChangesAsync(ct);

        try
        {
            // Phase A: Extract text.
            // For build simplicity the binary isn't persisted; the pipeline assumes the
            // FileHash already maps to in-memory download. We re-extract straight from
            // a stubbed byte provider when DocumentId is local-document. For MVP the file
            // was uploaded and extracted at endpoint time; we stored ExtractedText already.
            var extractedText = string.IsNullOrWhiteSpace(job.ExtractedText)
                ? await ExtractFromDocumentsModuleAsync(job, ct)
                : job.ExtractedText;

            if (string.IsNullOrWhiteSpace(extractedText))
            {
                job.SetFailed("FailedExtraction", "No text could be extracted from the uploaded file (likely scanned/image-only).");
                await _db.SaveChangesAsync(ct);
                return;
            }

            // Phase B: LLM parse.
            var parsed = await _parser.ParseAsync(extractedText, ct);
            if (!parsed.IsCV)
            {
                job.SetFailed("InvalidDocument", "Uploaded document was not detected as a CV.");
                await _db.SaveChangesAsync(ct);
                return;
            }

            // Phase C: Skill normalization (canonical skill ids; React/React.js collapse).
            var normalizedSkills = await _skillNormalizer.NormalizeAsync(parsed.Skills, ct);

            // Phase D: Create / resolve EmployeeProfile (status=PendingReview via factory default).
            // ExternalAccountId: use file hash so future re-uploads of the same CV are idempotent.
            var employeeResolution = await _getOrCreateEmployee.GetOrCreateAsync(
                organizationId: job.OrganizationId,
                sourceSystem: ExternalSourceSystem,
                externalAccountId: job.FileHash,
                externalDisplayName: parsed.Name ?? job.FileName,
                email: parsed.Email,
                jobTitle: parsed.JobTitle ?? "Unknown",
                linkedInUrl: parsed.LinkedInUrl,
                cancellationToken: ct);
            var employeeProfileId = employeeResolution.EmployeeProfileId;

            // Phase E: Persist parsed JSON on the job for downstream consumers (Assessment module will read it).
            var payload = new
            {
                employeeProfileId,
                normalizedSkills = normalizedSkills.Select(s => new { s.SkillId, s.SkillName, s.ConfidenceScore, s.Evidence, s.Source }),
                parsed.Experiences,
                parsed.Education,
                parsed.Certifications,
                parsed.Languages,
                parsed.Summary,
                parsed.JobTitle,
                parsed.Name,
                parsed.Email,
                parsed.Phone,
                parsed.LinkedInUrl,
                llmConfidence = parsed.LLMConfidence
            };
            job.SetSucceeded(employeeProfileId, System.Text.Json.JsonSerializer.Serialize(payload), null);

            // Phase F: Fire event for the Assessment module.
            _logger.LogInformation(
                "Publishing EmployeeProfilePendingReviewIntegrationEvent for employee profile {EmployeeProfileId}, organization {OrganizationId}, and CV parse job {CVParseJobId}.",
                employeeProfileId,
                job.OrganizationId,
                job.Id);

            await _mediator.Publish(
                new EmployeeProfilePendingReviewIntegrationEvent(employeeProfileId, job.OrganizationId, job.Id, parsed.Name, parsed.Email),
                ct);

            await _db.SaveChangesAsync(ct);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CV parse pipeline failed for job {JobId}.", jobId);
            var status = IsTransient(ex) ? "FailedTransient" : "FailedLLM";
            job.SetFailed(status, Truncate(ex.Message, 1000));
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task<string> ExtractFromDocumentsModuleAsync(CVParseJob job, CancellationToken ct)
    {
        // MVP placeholder: the Documents module stores metadata only.
        // Extracted text was filled at upload time by the endpoint via the aggregator.
        // If not present here, attempt to read from the persisted extracted text in
        // the CVParseJob row (it's the same row).
        await Task.CompletedTask;
        return job.ExtractedText ?? string.Empty;
    }

    private static bool IsTransient(Exception ex) => ex is HttpRequestException or TaskCanceledException or TimeoutException;
    private static string Truncate(string s, int n) => s.Length <= n ? s : s[..n];
}
