using Microsoft.EntityFrameworkCore;
using WorkFit.Payments.Contracts.Dtos;
using WorkFit.Payments.Features.Payments;
using WorkFit.Payments;
using WorkFit.Payments.Infrastructure.Data;
using WorkFit.Payments.Infrastructure.Gateways;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Payments.Features.Payments.CancelPayment;

public sealed class CancelPaymentCommandHandler : IRequestHandler<CancelPaymentCommand, PaymentDto>
{
    private readonly PaymentDbContext _context;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IPaymentDatabaseMigrator _databaseMigrator;

    public CancelPaymentCommandHandler(
        PaymentDbContext context,
        IPaymentGateway paymentGateway,
        IPaymentDatabaseMigrator databaseMigrator)
    {
        _context = context;
        _paymentGateway = paymentGateway;
        _databaseMigrator = databaseMigrator;
    }

    public async Task<PaymentDto> Handle(CancelPaymentCommand request, CancellationToken cancellationToken)
    {
        await _databaseMigrator.EnsureMigratedAsync(cancellationToken);

        var payment = await _context.Payments
            .SingleOrDefaultAsync(x => x.Id == request.PaymentId, cancellationToken)
            ?? throw new EntityNotFoundException(ModuleMarker.ModuleName, nameof(WorkFit.Payments.Domain.Entities.Payment), request.PaymentId);

        var gatewayResult = await _paymentGateway.CancelPaymentAsync(payment.ProviderPaymentId, cancellationToken);

        payment.UpdateGatewayState(
            gatewayResult.ProviderPaymentId,
            gatewayResult.TransactionId,
            gatewayResult.Status,
            gatewayResult.ClientSecret);

        await _context.SaveChangesAsync(cancellationToken);

        return payment.ToDto();
    }
}
