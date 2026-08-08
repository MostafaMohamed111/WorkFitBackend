using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using WorkFit.Organizations.Infrastructure.Data;
using WorkFit.Payments.Contracts.Enums;
using WorkFit.Payments.Domain.Entities;
using WorkFit.Payments.Infrastructure.Data;
using WorkFit.Payments.Infrastructure.Gateways;

namespace WorkFit.Payments.Features.Payments.CreateCheckoutSession;

public sealed class CreateCheckoutSessionEndpoint : Endpoint<CreateCheckoutSessionRequest, CreateCheckoutSessionResponse>
{
    private readonly PaymentDbContext _context;
    private readonly OrganizationDbContext _organizationContext;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IPaymentDatabaseMigrator _databaseMigrator;

    public CreateCheckoutSessionEndpoint(
        PaymentDbContext context,
        OrganizationDbContext organizationContext,
        IPaymentGateway paymentGateway,
        IPaymentDatabaseMigrator databaseMigrator)
    {
        _context = context;
        _organizationContext = organizationContext;
        _paymentGateway = paymentGateway;
        _databaseMigrator = databaseMigrator;
    }

    public override void Configure()
    {
        Post("/api/payments/checkout-session");
        AllowAnonymous();
        Options(x => x.WithTags("Payments"));
        Description(static b => b
            .Produces<CreateCheckoutSessionResponse>(200)
            .ProducesProblem(400));
    }

    public override async Task HandleAsync(CreateCheckoutSessionRequest req, CancellationToken ct)
    {
        await _databaseMigrator.EnsureMigratedAsync(ct);

        if (!Guid.TryParse(req.ReferenceId, out var organizationId)
            || !await _organizationContext.Organizations.AnyAsync(x => x.Id == organizationId, ct))
        {
            ThrowError("The supplied referenceId does not correspond to an existing organization.");
            return;
        }

        var payment = await _context.Payments
            .SingleOrDefaultAsync(
                x => x.ReferenceId == req.ReferenceId && x.ReferenceType == req.ReferenceType,
                ct);

        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
        var successUrl = $"{baseUrl}/success.html?session_id={{CHECKOUT_SESSION_ID}}";
        var cancelUrl = $"{baseUrl}/cancel.html?reference_id={Uri.EscapeDataString(req.ReferenceId)}&reference_type={Uri.EscapeDataString(req.ReferenceType)}";

        var result = await _paymentGateway.CreateCheckoutSessionAsync(
            new PaymentGatewayRequest(
                req.Amount,
                req.Currency,
                req.ReferenceId,
                req.ReferenceType,
                req.Description,
                new Dictionary<string, string>
                {
                    ["reference_id"] = req.ReferenceId,
                    ["reference_type"] = req.ReferenceType,
                    ["plan_name"] = string.IsNullOrWhiteSpace(req.PlanName) ? "Basic" : req.PlanName,
                    ["billing_cycle"] = string.IsNullOrWhiteSpace(req.BillingCycle) ? "Onetime" : req.BillingCycle
                },
                null),
            successUrl,
            cancelUrl,
            ct);

        if (payment is null)
        {
            payment = Payment.Create(
                req.ReferenceId,
                req.ReferenceType,
                req.Amount,
                req.Currency,
                _paymentGateway.Provider,
                result.SessionId,
                null,
                PaymentStatus.Pending,
                null);

            _context.Add(payment);
        }
        else
        {
            payment.UpdateGatewayState(result.SessionId, null, PaymentStatus.Pending);
        }

        await _context.SaveChangesAsync(ct);

        await Send.OkAsync(new CreateCheckoutSessionResponse(
            payment.Id,
            payment.ReferenceId,
            payment.ReferenceType,
            payment.Amount,
            payment.Currency,
            payment.Status,
            payment.Provider,
            payment.ProviderPaymentId,
            payment.TransactionId,
            payment.ClientSecret,
            payment.CreatedAt,
            payment.UpdatedAt,
            result.SessionId,
            result.Url), ct);
    }
}
