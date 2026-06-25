
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WorkFit.SharedKernel.RegisterModuleServices;

public interface IRegisterModuleServices
{
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
}
