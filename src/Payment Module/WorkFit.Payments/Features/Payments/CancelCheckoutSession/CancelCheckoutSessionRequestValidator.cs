using FastEndpoints;
using FluentValidation;

namespace WorkFit.Payments.Features.Payments.CancelCheckoutSession;

public sealed class CancelCheckoutSessionRequestValidator : Validator<CancelCheckoutSessionRequest>
{
    public CancelCheckoutSessionRequestValidator()
    {
        RuleFor(x => x.ReferenceId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ReferenceType)
            .NotEmpty()
            .MaximumLength(100);
    }
}
