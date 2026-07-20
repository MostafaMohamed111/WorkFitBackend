using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;
using WorkFit.Payments.Infrastructure.Configuration;
using WorkFit.Payments.Infrastructure.Data;
using WorkFit.Payments.Infrastructure.Gateways;
using WorkFit.Payments.Infrastructure.Services;

namespace WorkFit.Payments;

public sealed class RegisterPaymentModuleServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.Configure<PaymentOptions>(configuration.GetSection("Payments"));

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddSingleton<IPaymentDatabaseMigrator, PaymentDatabaseMigrator>();
        services.AddHostedService<PaymentDatabaseInitializer>();

        services.AddSingleton<StripePaymentGateway>();
        services.AddSingleton<MockPaymentGateway>();
        services.AddSingleton<IPaymentGateway>(sp =>
        {
            var paymentOptions = sp.GetRequiredService<IOptions<PaymentOptions>>().Value;
            var provider = paymentOptions.Provider?.Trim();

            if (string.Equals(provider, PaymentProviderName.Stripe, StringComparison.OrdinalIgnoreCase))
            {
                return sp.GetRequiredService<StripePaymentGateway>();
            }

            if (string.Equals(provider, PaymentProviderName.Mock, StringComparison.OrdinalIgnoreCase))
            {
                return sp.GetRequiredService<MockPaymentGateway>();
            }

            throw new InvalidOperationException(
                $"Unsupported payment provider '{paymentOptions.Provider}'.");
        });

        services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();

        services.AddMediatorHandlers<ModuleMarker>();
    }
}
