using WorkFit.Payments.Contracts.Dtos;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Payments.Features.Payments.GetPaymentById;

public sealed record GetPaymentByIdQuery(Guid PaymentId) : IRequest<PaymentDto>;
