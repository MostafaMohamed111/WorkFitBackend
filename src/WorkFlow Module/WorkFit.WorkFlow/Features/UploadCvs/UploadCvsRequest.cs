using Microsoft.AspNetCore.Http;

namespace WorkFit.WorkFlow.Features.UploadCvs;

public sealed class UploadCvsRequest
{
    public Guid OrganizationId { get; set; }

    // Accepts any mix of loose PDF/DOCX files and ZIP archives in one field.
    public List<IFormFile>? Files { get; set; }
}