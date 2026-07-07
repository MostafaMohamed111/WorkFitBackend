

using Microsoft.Extensions.DependencyInjection;
using WorkFit.SharedKernel.MediatorContract;

namespace WorkFit.SharedKernel.DependencyInjection;

public static class MediatorServiceCollectionRegisteration
{
    public static IServiceCollection AddMediatorHandlers<TMarker>(this IServiceCollection services)
    {
        var assembly = typeof(TMarker).Assembly;

        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)), false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            )
            .Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<>)), false)
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            )
            .Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IIntegrationEventHandler<>)), false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }
}
