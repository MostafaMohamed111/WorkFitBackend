using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Features.Queries.GetBatchStatus;

internal sealed record GetBatchStatusQuery(Guid BatchId) : IRequest<IReadOnlyList<CVParseJobStatusDto>>;