using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WorkFit.Email.Contracts;
using WorkFit.Email.Infrastructure;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;

namespace WorkFit.Email;

internal sealed class RegisterEmailModuleServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediatorHandlers<ModuleMarker>();

        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName))
            .Validate(options => options.Port is > 0 and <= 65535, "SMTP port must be between 1 and 65535.")
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Host), "SMTP host is required.")
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.From), "Email sender address is required.")
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Username), "SMTP username is required.")
            .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Password), "SMTP password is required.")
            .ValidateOnStart();

        services.AddScoped<ISendEmailService, SmtpEmailService>();
    }
}
