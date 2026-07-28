using FastEndpoints;
using FluentValidation;

namespace WorkFit.Payments.Features.Payments.CreateCheckoutSession;

public sealed class CreateCheckoutSessionRequestValidator : Validator<CreateCheckoutSessionRequest>
{
    public CreateCheckoutSessionRequestValidator()
    {
        RuleFor(x => x.ReferenceId)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ReferenceType)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3);

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.PlanName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.PlanName));
    }
}
