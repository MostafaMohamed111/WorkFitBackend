using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using WorkFit.Payments.Contracts.Dtos;
using WorkFit.Payments.Domain.Entities;
using WorkFit.Payments.Features.Payments;
using WorkFit.Payments;
using WorkFit.Payments.Infrastructure.Data;
using WorkFit.Payments.Infrastructure.Gateways;
using WorkFit.SharedKernel.Exceptions.FeatureExceptions;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Payments.Features.Payments.CreatePayment;

public sealed class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    private readonly PaymentDbContext _context;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IPaymentDatabaseMigrator _databaseMigrator;

    public CreatePaymentCommandHandler(
        PaymentDbContext context,
        IPaymentGateway paymentGateway,
        IPaymentDatabaseMigrator databaseMigrator)
    {
        _context = context;
        _paymentGateway = paymentGateway;
        _databaseMigrator = databaseMigrator;
    }

    public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        await _databaseMigrator.EnsureMigratedAsync(cancellationToken);

        var existingPayment = await _context.Payments
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.ReferenceId == request.ReferenceId && x.ReferenceType == request.ReferenceType,
                cancellationToken);

        if (existingPayment is not null)
        {
            throw new EntityAlreadyExistsException(
                ModuleMarker.ModuleName,
                nameof(Payment),
                existingPayment.Id);
        }

        var gatewayResult = await _paymentGateway.CreatePaymentIntentAsync(
            new PaymentGatewayRequest(
                request.Amount,
                request.Currency,
                request.ReferenceId,
                request.ReferenceType,
                request.Description,
                new Dictionary<string, string>
                {
                    ["reference_id"] = request.ReferenceId,
                    ["reference_type"] = request.ReferenceType,
                    ["plan_name"] = string.IsNullOrWhiteSpace(request.PlanName) ? "Basic" : request.PlanName
                },
                request.MockOutcome),
            cancellationToken);

        var payment = await _context.Payments.SingleOrDefaultAsync(
            x => x.ProviderPaymentId == gatewayResult.ProviderPaymentId,
            cancellationToken);

        if (payment is null)
        {
            payment = Payment.Create(
                request.ReferenceId,
                request.ReferenceType,
                request.Amount,
                request.Currency,
                _paymentGateway.Provider,
                gatewayResult.ProviderPaymentId,
                gatewayResult.TransactionId,
                gatewayResult.Status,
                gatewayResult.ClientSecret);

            _context.Add(payment);
        }
        else
        {
            payment.UpdateGatewayState(
                gatewayResult.ProviderPaymentId,
                gatewayResult.TransactionId,
                gatewayResult.Status,
                gatewayResult.ClientSecret);
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicatePaymentProviderId(ex))
        {
            _context.Entry(payment).State = EntityState.Detached;

            var persistedPayment = await _context.Payments
                .SingleAsync(x => x.ProviderPaymentId == gatewayResult.ProviderPaymentId, cancellationToken);

            persistedPayment.UpdateGatewayState(
                gatewayResult.ProviderPaymentId,
                gatewayResult.TransactionId,
                gatewayResult.Status,
                gatewayResult.ClientSecret);

            await _context.SaveChangesAsync(cancellationToken);
            payment = persistedPayment;
        }

        return payment.ToDto();
    }

    private static bool IsDuplicatePaymentProviderId(DbUpdateException exception)
    {
        if (exception.InnerException is SqlException sqlException)
        {
            return sqlException.Number is 2601 or 2627;
        }

        return exception.InnerException?.Message.Contains("IX_payments_ProviderPaymentId", StringComparison.OrdinalIgnoreCase) == true
            || exception.InnerException?.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true;
    }
}
