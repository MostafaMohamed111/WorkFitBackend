using WorkFit.Payments.Contracts.Dtos;
using WorkFit.Payments.Domain.Entities;

namespace WorkFit.Payments.Features.Payments;

internal static class PaymentMappings
{
    public static PaymentDto ToDto(this Payment payment)
    {
        return new PaymentDto(
            payment.Id,
            payment.ReferenceId,
            payment.ReferenceType,
            payment.Amount,
            payment.Currency,
            payment.Status,
            payment.Provider,
            payment.ProviderPaymentId,
            payment.TransactionId,
            payment.ClientSecret,
            payment.CreatedAt,
            payment.UpdatedAt);
    }

    public static PaymentStatusDto ToStatusDto(this Payment payment)
    {
        return new PaymentStatusDto(
            payment.Id,
            payment.Status,
            payment.Provider,
            payment.ProviderPaymentId,
            payment.TransactionId,
            payment.UpdatedAt);
    }
}
