using Microsoft.EntityFrameworkCore;
using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.Engine.Domain.Entities;
using WorkFit.Engine.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Features.Queries.GetCVParseJobStatus;

internal sealed class GetCVParseJobStatusQueryHandler : IRequestHandler<GetCVParseJobStatusQuery, CVParseJobStatusDto>
{
    private readonly EngineDbContext _db;

    public GetCVParseJobStatusQueryHandler(EngineDbContext db)
    {
        _db = db;
    }

    public async Task<CVParseJobStatusDto> Handle(GetCVParseJobStatusQuery query, CancellationToken cancellationToken = default)
    {
        var job = await _db.CVParseJobs.FirstOrDefaultAsync(j => j.Id == query.JobId, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, nameof(CVParseJob), query.JobId);

        return new CVParseJobStatusDto(
            job.Id,
            job.BatchId,
            job.EmployeeProfileId,
            Enum.Parse<CVParseJobStatus>(job.Status),
            job.Error,
            job.Attempts,
            job.TokenUsage,
            job.CreatedAt,
            job.CompletedAt);
    }
}