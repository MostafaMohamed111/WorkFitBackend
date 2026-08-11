using Microsoft.AspNetCore.Http;

namespace WorkFit.Engine.Features.Commands.UploadBulkCV;

public sealed class UploadBulkCVRequest
{
    public IFormFile Zip { get; set; } = null!;
}