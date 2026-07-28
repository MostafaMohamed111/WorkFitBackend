namespace WorkFit.Payments.Infrastructure.Services;

public interface IOrganizationSubscriptionService
{
    Task ActivateSubscriptionAsync(
        Guid organizationId,
        Guid paymentId,
        string planName,
        bool isRecurring,
        string billingCycle,
        CancellationToken cancellationToken = default);
}
