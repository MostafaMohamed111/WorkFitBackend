using WorkFit.Payments.Contracts.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Payments.Features.Payments.GetPaymentStatus;

public sealed record GetPaymentStatusQuery(Guid PaymentId) : IRequest<PaymentStatusDto>;
