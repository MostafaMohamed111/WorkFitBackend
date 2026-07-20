using FastEndpoints;
using FluentValidation;

namespace WorkFit.Payments.Features.Payments.ConfirmPayment;

public sealed class ConfirmPaymentRequestValidator : Validator<ConfirmPaymentRequest>
{
    public ConfirmPaymentRequestValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty();
    }
}
