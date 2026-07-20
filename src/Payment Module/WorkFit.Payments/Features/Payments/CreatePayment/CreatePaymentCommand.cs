using WorkFit.Payments.Contracts.Dtos;
using WorkFit.Payments.Contracts.Enums;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Payments.Features.Payments.CreatePayment;

public sealed record CreatePaymentCommand(
    string ReferenceId,
    string ReferenceType,
    decimal Amount,
    string Currency,
    string? Description,
    MockPaymentOutcome? MockOutcome) : IRequest<PaymentDto>;
