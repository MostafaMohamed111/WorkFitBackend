using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Features.Commands.RetryCVParseJob;

internal sealed record RetryCVParseJobCommand(Guid JobId) : IRequest<UploadCVResponse>;