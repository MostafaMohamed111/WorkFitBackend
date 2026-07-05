
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;

namespace WorkFit.WorkFlow
{
    internal class RegisterWorkFlowModuleServices : IRegisterModuleServices
    {
        public void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatorHandlers<ModuleMarker>();
        }
    }
}
