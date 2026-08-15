using Microsoft.AspNetCore.Http;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.WorkFlow.Features.UploadCvs;

public sealed record UploadCvsCommand(
    Guid OrganizationId,
    IReadOnlyList<IFormFile> Files) : IRequest<UploadCvsResponse>;
