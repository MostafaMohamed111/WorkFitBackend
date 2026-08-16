using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.WorkFlow.Features.UploadCvs;

public sealed record UploadCvsCommand(
    IReadOnlyList<IFormFile> Files) : IRequest<UploadCvsResponse>;
