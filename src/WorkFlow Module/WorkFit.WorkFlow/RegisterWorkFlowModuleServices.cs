
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;
using Microsoft.EntityFrameworkCore;
using WorkFit.WorkFlow.Invitations;
using WorkFit.Rag.Contracts.Agent;
using WorkFit.WorkFlow.Features.AgentChat;

namespace WorkFit.WorkFlow
{
    internal class RegisterWorkFlowModuleServices : IRegisterModuleServices
    {
        public void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatorHandlers<ModuleMarker>();
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<InvitationDbContext>(options => options.UseSqlServer(connectionString));
            services.Configure<InvitationEmailOptions>(configuration.GetSection("DeveloperInvitations:Email"));
            services.AddScoped<InvitationEmailSender>();
            services.AddScoped<InvitationService>();
            services.AddScoped<IAgentChatService, AgentChatService>();
        }
    }
}
