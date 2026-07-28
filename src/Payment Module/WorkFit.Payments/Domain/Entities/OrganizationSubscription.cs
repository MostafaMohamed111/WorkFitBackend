namespace WorkFit.Payments.Domain.Entities;

public sealed class OrganizationSubscription
{
    private OrganizationSubscription()
    {
    }

    private OrganizationSubscription(Guid organizationId, string planName)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        PlanName = planName;
        Status = "Pending";
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid OrganizationId { get; private set; }

    public string PlanName { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? ActivatedAt { get; private set; }

    public DateTimeOffset? ExpiresAt { get; private set; }

    public Guid? PaymentId { get; private set; }

    public static OrganizationSubscription Create(Guid organizationId, string planName, Guid? paymentId = null)
    {
        if (organizationId == Guid.Empty)
        {
            throw new ArgumentException("OrganizationId must not be empty.", nameof(organizationId));
        }

        var subscription = new OrganizationSubscription(organizationId, string.IsNullOrWhiteSpace(planName) ? "Basic" : planName)
        {
            PaymentId = paymentId
        };

        return subscription;
    }

    public void Activate(string planName, Guid? paymentId = null)
    {
        PlanName = string.IsNullOrWhiteSpace(planName) ? "Basic" : planName;
        Status = "Active";
        ActivatedAt = DateTimeOffset.UtcNow;
        ExpiresAt ??= DateTimeOffset.UtcNow.AddMonths(1);
        PaymentId = paymentId ?? PaymentId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
