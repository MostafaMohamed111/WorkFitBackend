using Microsoft.EntityFrameworkCore;
using WorkFit.Payments.Contracts.Dtos;
using WorkFit.Payments.Domain.Entities;
using WorkFit.Payments.Features.Payments;
using WorkFit.Payments.Infrastructure.Data;
using WorkFit.Payments.Infrastructure.Gateways;
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
                    ["reference_type"] = request.ReferenceType
                },
                request.MockOutcome),
            cancellationToken);

        var payment = Payment.Create(
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
        await _context.SaveChangesAsync(cancellationToken);

        return payment.ToDto();
    }
}
