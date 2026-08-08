using FastEndpoints;
using FluentValidation;

namespace WorkFit.Payments.Features.Payments.CreatePayment;

public sealed class CreatePaymentRequestValidator : Validator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.ReferenceId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ReferenceType)
            .NotEmpty()
            .MaximumLength(100)
            .Must(x => string.Equals(x, "Organization", StringComparison.OrdinalIgnoreCase)
                || string.Equals(x, "Project", StringComparison.OrdinalIgnoreCase))
            .WithMessage("ReferenceType must be either 'Organization' or 'Project'.");

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(3);
    }
}
