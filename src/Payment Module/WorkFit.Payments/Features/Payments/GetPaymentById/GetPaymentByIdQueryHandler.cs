using Microsoft.EntityFrameworkCore;
using WorkFit.Payments.Contracts.Dtos;
using WorkFit.Payments.Features.Payments;
using WorkFit.Payments;
using WorkFit.Payments.Infrastructure.Data;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Payments.Features.Payments.GetPaymentById;

public sealed class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto>
{
    private readonly PaymentDbContext _context;
    private readonly IPaymentDatabaseMigrator _databaseMigrator;

    public GetPaymentByIdQueryHandler(
        PaymentDbContext context,
        IPaymentDatabaseMigrator databaseMigrator)
    {
        _context = context;
        _databaseMigrator = databaseMigrator;
    }

    public async Task<PaymentDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        await _databaseMigrator.EnsureMigratedAsync(cancellationToken);

        var payment = await _context.Payments
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == request.PaymentId, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, nameof(WorkFit.Payments.Domain.Entities.Payment), request.PaymentId);

        return payment.ToDto();
    }
}
