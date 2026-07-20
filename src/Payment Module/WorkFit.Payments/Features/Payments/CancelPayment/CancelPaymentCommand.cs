using WorkFit.Payments.Contracts.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Payments.Features.Payments.CancelPayment;

public sealed record CancelPaymentCommand(Guid PaymentId) : IRequest<PaymentDto>;
