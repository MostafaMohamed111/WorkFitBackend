using Microsoft.EntityFrameworkCore;
using WorkFit.Payments.Domain.Entities;
using WorkFit.Payments.Infrastructure.Data;

namespace WorkFit.Payments.Infrastructure.Services;

public sealed class OrganizationSubscriptionService : IOrganizationSubscriptionService
{
    private readonly PaymentDbContext _context;

    public OrganizationSubscriptionService(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task ActivateSubscriptionAsync(Guid organizationId, Guid paymentId, string planName, CancellationToken cancellationToken = default)
    {
        var subscription = await _context.Set<OrganizationSubscription>()
            .SingleOrDefaultAsync(x => x.OrganizationId == organizationId, cancellationToken);

        if (subscription is null)
        {
            subscription = OrganizationSubscription.Create(organizationId, planName, paymentId);
            _context.Add(subscription);
        }
        else
        {
            subscription.Activate(planName, paymentId);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
