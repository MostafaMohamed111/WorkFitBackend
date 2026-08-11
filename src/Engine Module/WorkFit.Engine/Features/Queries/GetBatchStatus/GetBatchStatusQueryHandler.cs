using Microsoft.EntityFrameworkCore;
using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.Engine.Infrastructure.Data;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Features.Queries.GetBatchStatus;

internal sealed class GetBatchStatusQueryHandler : IRequestHandler<GetBatchStatusQuery, IReadOnlyList<CVParseJobStatusDto>>
{
    private readonly EngineDbContext _db;

    public GetBatchStatusQueryHandler(EngineDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CVParseJobStatusDto>> Handle(GetBatchStatusQuery query, CancellationToken cancellationToken = default)
    {
        var jobs = await _db.CVParseJobs
            .Where(j => j.BatchId == query.BatchId)
            .Select(j => new CVParseJobStatusDto(
                j.Id, j.BatchId, j.EmployeeProfileId,
                Enum.Parse<CVParseJobStatus>(j.Status), j.Error, j.Attempts, j.TokenUsage,
                j.CreatedAt, j.CompletedAt))
            .ToListAsync(cancellationToken);

        return jobs;
    }
}