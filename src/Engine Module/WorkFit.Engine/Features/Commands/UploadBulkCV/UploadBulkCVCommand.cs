using Microsoft.AspNetCore.Http;
using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Features.Commands.UploadBulkCV;

internal sealed record UploadBulkCVCommand(IFormFile Zip) : IRequest<UploadCVBulkResponse>;