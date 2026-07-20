using WorkFit.Payments.Contracts.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Payments.Features.Payments.ConfirmPayment;

public sealed record ConfirmPaymentCommand(Guid PaymentId) : IRequest<PaymentDto>;
