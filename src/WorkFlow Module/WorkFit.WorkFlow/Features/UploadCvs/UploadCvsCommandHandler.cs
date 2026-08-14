using System.IO.Compression;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using WorkFit.Assessments.Contracts.CreateAssessmentService;
using WorkFit.Documents.Contracts.DocumentContentService;
using WorkFit.Documents.Contracts.TemporaryUploadService;
using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.Identity.Contracts.IdentityServices;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.TalentManagement.Contracts.WriteServices.CreateEmployee;

namespace WorkFit.WorkFlow.Features.UploadCvs;

public sealed partial class UploadCvsCommandHandler : IRequestHandler<UploadCvsCommand, UploadCvsResponse>
{
    private static readonly HashSet<string> SupportedCvExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".docx"
    };

    private readonly ICreateTemporaryDocumentService _createTemporaryDocumentService;
    private readonly IDocumentContentService _documentContentService;
    private readonly IParseCVDocumentsService _parseCVDocumentsService;
    private readonly ICreateEmployeeUserService _createEmployeeUserService;
    private readonly ICreateEmployeeService _createEmployeeService;
    private readonly ICreateAssessmentService _createAssessmentService;

    public UploadCvsCommandHandler(
        ICreateTemporaryDocumentService createTemporaryDocumentService,
        IDocumentContentService documentContentService,
        IParseCVDocumentsService parseCVDocumentsService,
        ICreateEmployeeUserService createEmployeeUserService,
        ICreateEmployeeService createEmployeeService,
        ICreateAssessmentService createAssessmentService)
    {
        _createTemporaryDocumentService = createTemporaryDocumentService;
        _documentContentService = documentContentService;
        _parseCVDocumentsService = parseCVDocumentsService;
        _createEmployeeUserService = createEmployeeUserService;
        _createEmployeeService = createEmployeeService;
        _createAssessmentService = createAssessmentService;
    }

    public async Task<UploadCvsResponse> Handle(UploadCvsCommand request, CancellationToken ct = default)
    {
        var items = new List<UploadCvsItemResult>();

        // Both direct files and a ZIP archive can be uploaded together; every entry
        // (loose file or zipped file) is treated identically from here on.
        var candidates = await CollectCandidatesAsync(request, items, ct);

        var uploadedDocuments = await StoreCandidatesAsync(request, candidates, items, ct);

        if (uploadedDocuments.Count == 0)
        {
            throw new FeatureException(
                ModuleMarker.ModuleName,
                "NO_SUPPORTED_CV_FILES",
                "No supported CV files were uploaded.",
                "Upload one or more PDF/DOCX CV files directly or inside a ZIP archive.");
        }

        var parseInputs = new List<ParsedCVDocumentInput>(uploadedDocuments.Count);
        var openedStreams = new List<Stream>(uploadedDocuments.Count);

        try
        {
            foreach (var doc in uploadedDocuments)
            {
                var content = await _documentContentService.OpenReadAsync(doc.documentId, ct);
                openedStreams.Add(content.Content);
                parseInputs.Add(new ParsedCVDocumentInput(
                    content.Id,
                    content.FileName,
                    content.ContentType,
                    content.Content));
            }

            var parsedResults = await _parseCVDocumentsService.ParseAsync(parseInputs, ct);
            foreach (var parsedResult in parsedResults)
            {
                var fileName = uploadedDocuments
                    .First(x => x.documentId == parsedResult.DocumentId)
                    .fileName;

                if (!parsedResult.Success || parsedResult.ParsedCV is null)
                {
                    items.Add(new UploadCvsItemResult(
                        parsedResult.DocumentId,
                        fileName,
                        false,
                        parsedResult.Error ?? "Parsing failed.",
                        null,
                        null,
                        null,
                        null));
                    continue;
                }

                var parsed = parsedResult.ParsedCV;
                if (string.IsNullOrWhiteSpace(parsed.Email))
                {
                    items.Add(new UploadCvsItemResult(
                        parsedResult.DocumentId,
                        fileName,
                        false,
                        "Parsed CV did not contain an email.",
                        null,
                        null,
                        null,
                        null));
                    continue;
                }

                var displayName = string.IsNullOrWhiteSpace(parsed.Name)
                    ? parsed.Email.Split('@')[0]
                    : parsed.Name;

                var employeePassword = GenerateStrongPassword();

                var identity = await _createEmployeeUserService.GetOrCreateAsync(
                    parsed.Email,
                    displayName,
                    employeePassword,
                    cancellationToken: ct);

                var employeeProfileId = await _createEmployeeService.CreateEmployeeAsync(
                    new EmployeeDetails(
                        organizationId: request.OrganizationId,
                        userId: identity.UserId,
                        email: parsed.Email,
                        name: displayName,
                        jobTitle: parsed.JobTitle ?? "Unknown"),
                    ct);

                var skillChanges = parsedResult.NormalizedSkills
                    .Select(s => (
                        skillId: s.SkillId,
                        skillName: s.SkillName,
                        oldScore: 0,
                        proposedScore: Math.Clamp(s.ConfidenceScore, 0, 100),
                        evidenceDesc: s.Evidence ?? "Extracted from uploaded CV"))
                    .ToList();

                var assessmentId = await _createAssessmentService.CreateAsync(
                    employeeProfileId: employeeProfileId,
                    employeeUserId: identity.UserId,
                    description: "Auto-generated assessment from CV parsing workflow upload.",
                    type: AssessmentType.EmployeeProfileSelfAssessment,
                    skillChanges: skillChanges,
                    taskId: null,
                    teamLeadId: null);

                items.Add(new UploadCvsItemResult(
                    parsedResult.DocumentId,
                    fileName,
                    true,
                    null,
                    identity.UserId,
                    employeeProfileId,
                    assessmentId,
                    identity.GeneratedPassword));
            }
        }
        finally
        {
            foreach (var stream in openedStreams)
                stream.Dispose();
        }

        var total = items.Count;
        var succeeded = items.Count(i => i.Success);
        return new UploadCvsResponse(total, succeeded, total - succeeded, items);
    }

    private async Task<List<CvCandidate>> CollectCandidatesAsync(
        UploadCvsCommand request,
        List<UploadCvsItemResult> items,
        CancellationToken ct)
    {
        var candidates = new List<CvCandidate>();

        foreach (var file in request.Files)
        {
            if (file is null || file.Length == 0)
                continue;

            if (IsZip(file.FileName))
            {
                await ExtractZipCandidatesAsync(file, candidates, items, ct);
                continue;
            }

            if (!IsSupportedCv(file.FileName))
            {
                items.Add(UnsupportedItem(file.FileName));
                continue;
            }

            candidates.Add(new CvCandidate(file.FileName, file.ContentType, file.OpenReadStream(), file.Length));
        }

        return candidates;
    }

    private async Task ExtractZipCandidatesAsync(
        IFormFile zip,
        List<CvCandidate> candidates,
        List<UploadCvsItemResult> items,
        CancellationToken ct)
    {
        await using var zipStream = zip.OpenReadStream();
        using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        foreach (var entry in zipArchive.Entries)
        {
            ct.ThrowIfCancellationRequested();
            if (entry.Length == 0)
                continue;

            if (!IsSupportedCv(entry.Name))
            {
                items.Add(UnsupportedItem(entry.Name));
                continue;
            }

            // Zip entry streams don't survive past the archive's lifetime, so materialize them.
            await using var entryStream = entry.Open();
            var memory = new MemoryStream();
            await entryStream.CopyToAsync(memory, ct);
            memory.Position = 0;

            candidates.Add(new CvCandidate(entry.Name, ResolveContentType(entry.Name), memory, entry.Length));
        }
    }

    private async Task<List<(Guid documentId, string fileName)>> StoreCandidatesAsync(
        UploadCvsCommand request,
        List<CvCandidate> candidates,
        List<UploadCvsItemResult> items,
        CancellationToken ct)
    {
        var uploadedDocuments = new List<(Guid documentId, string fileName)>();

        foreach (var candidate in candidates)
        {
            try
            {
                if (candidate.Content.CanSeek)
                    candidate.Content.Position = 0;

                var created = await _createTemporaryDocumentService.CreateAsync(
                    candidate.Content,
                    candidate.FileName,
                    candidate.ContentType,
                    candidate.Size,
                    request.OrganizationId,
                    ct);

                uploadedDocuments.Add((created.Id, created.FileName));
            }
            catch (Exception ex)
            {
                items.Add(FailedItem(candidate.FileName, ex.Message));
            }
            finally
            {
                await candidate.Content.DisposeAsync();
            }
        }

        return uploadedDocuments;
    }

    private static bool IsSupportedCv(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return !string.IsNullOrWhiteSpace(ext) && SupportedCvExtensions.Contains(ext);
    }

    private static bool IsZip(string fileName) =>
        Path.GetExtension(fileName).Equals(".zip", StringComparison.OrdinalIgnoreCase);

    private static string ResolveContentType(string fileName) =>
        Path.GetExtension(fileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase)
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    private static UploadCvsItemResult UnsupportedItem(string fileName) => new(
        null, fileName, false, "Unsupported file extension. Only PDF and DOCX are supported.", null, null, null, null);

    private static UploadCvsItemResult FailedItem(string fileName, string error) => new(
        null, fileName, false, error, null, null, null, null);

    private static string GenerateStrongPassword()
    {
        const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lowercase = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%^&*()-_=+[]{}";
        var all = uppercase + lowercase + digits + symbols;

        Span<char> password = stackalloc char[16];
        password[0] = uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)];
        password[1] = lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)];
        password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
        password[3] = symbols[RandomNumberGenerator.GetInt32(symbols.Length)];

        for (var i = 4; i < password.Length; i++)
            password[i] = all[RandomNumberGenerator.GetInt32(all.Length)];

        for (var i = password.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password);
    }
}
