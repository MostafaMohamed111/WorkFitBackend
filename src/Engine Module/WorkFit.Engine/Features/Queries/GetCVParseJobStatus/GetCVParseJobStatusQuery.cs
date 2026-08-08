using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Features.Queries.GetCVParseJobStatus;

internal sealed record GetCVParseJobStatusQuery(Guid JobId) : IRequest<CVParseJobStatusDto>;