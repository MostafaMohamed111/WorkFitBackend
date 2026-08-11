using Microsoft.EntityFrameworkCore;
using WorkFit.Engine.Contracts.CVParsing;
using WorkFit.Engine.Domain.Entities;
using WorkFit.Engine.Infrastructure.CVParsing;
using WorkFit.Engine.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Engine.Features.Commands.RetryCVParseJob;

internal sealed class RetryCVParseJobCommandHandler : IRequestHandler<RetryCVParseJobCommand, UploadCVResponse>
{
    private readonly EngineDbContext _db;
    private readonly CVProcessingChannel _channel;

    public RetryCVParseJobCommandHandler(EngineDbContext db, CVProcessingChannel channel)
    {
        _db = db;
        _channel = channel;
    }

    public async Task<UploadCVResponse> Handle(RetryCVParseJobCommand command, CancellationToken cancellationToken = default)
    {
        var job = await _db.CVParseJobs.FirstOrDefaultAsync(j => j.Id == command.JobId, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, nameof(CVParseJob), command.JobId);

        if (job.Status is "Queued" or "Processing")
            return new UploadCVResponse(job.Id, Enum.Parse<CVParseJobStatus>(job.Status));

        job.Requeue();
        await _db.SaveChangesAsync(cancellationToken);
        await _channel.EnqueueAsync(new ProcessCVJobMessage(job.Id), cancellationToken);

        return new UploadCVResponse(job.Id, Enum.Parse<CVParseJobStatus>(job.Status));
    }
}