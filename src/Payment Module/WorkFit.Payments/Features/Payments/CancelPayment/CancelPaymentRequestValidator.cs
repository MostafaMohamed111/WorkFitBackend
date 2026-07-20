using FastEndpoints;
using FluentValidation;

namespace WorkFit.Payments.Features.Payments.CancelPayment;

public sealed class CancelPaymentRequestValidator : Validator<CancelPaymentRequest>
{
    public CancelPaymentRequestValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty();
    }
}
