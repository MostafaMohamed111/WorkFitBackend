using System.Reflection;
using WorkFit.SharedKernel.RegisterModuleServices;

namespace WorkFit.Host.ExtentionMethods;

public static class RegisterModulesExtention 
{
    public static IServiceCollection RegisterModules(this IServiceCollection services, IConfiguration configuration, Assembly[] assembliesToScan)
    {
        var assemblies = assembliesToScan.ToList();

        var moduleRegistrars = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IRegisterModuleServices).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<IRegisterModuleServices>();

        foreach (var registrar in moduleRegistrars)
            registrar.RegisterServices(services, configuration);

        return services;
    }
}
