using Microsoft.AspNetCore.Http;

namespace WorkFit.Engine.Features.Commands.UploadSingleCV;

public sealed class UploadSingleCVRequest
{
    public IFormFile File { get; set; } = null!;
}