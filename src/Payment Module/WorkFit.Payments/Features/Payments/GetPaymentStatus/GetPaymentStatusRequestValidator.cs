using FastEndpoints;
using FluentValidation;

namespace WorkFit.Payments.Features.Payments.GetPaymentStatus;

public sealed class GetPaymentStatusRequestValidator : Validator<GetPaymentStatusRequest>
{
    public GetPaymentStatusRequestValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty();
    }
}
