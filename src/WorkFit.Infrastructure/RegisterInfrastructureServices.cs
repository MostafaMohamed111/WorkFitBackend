
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.Infrastructure.CurrentUser;
using WorkFit.Infrastructure.MediatorImp;
using WorkFit.SharedKernel.ICurrentUser;
using WorkFit.SharedKernel.MediatorContract;
using WorkFit.SharedKernel.RegisterModuleServices;

namespace WorkFit.Infrastructure;

public class RegisterInfrastructureServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMediator, Mediator>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();
    }
}
