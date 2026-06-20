
using Microsoft.Extensions.DependencyInjection;
using WorkFit.Infrastructure.MediatorImp;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IMediator, Mediator>();

        return services;
    }
}
