using FastEndpoints;
using FluentValidation;

namespace WorkFit.Payments.Features.Payments.GetPaymentById;

public sealed class GetPaymentByIdRequestValidator : Validator<GetPaymentByIdRequest>
{
    public GetPaymentByIdRequestValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty();
    }
}
