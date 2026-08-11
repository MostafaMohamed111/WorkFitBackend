using Microsoft.AspNetCore.Http;
using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Features.Commands.UploadSingleCV;

internal sealed record UploadSingleCVCommand(IFormFile File) : IRequest<UploadCVResponse>;