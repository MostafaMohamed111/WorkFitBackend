using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkFit.CodeReview.Contracts.GitHubCodeReview;
using WorkFit.CodeReview.Features.GitHubCodeReview;
using WorkFit.CodeReview.Infrastructure.Data;
using WorkFit.CodeReview.Infrastructure.Repositories;
using WorkFit.CodeReview.Infrastructure.Services;
using WorkFit.SharedKernel.DependencyInjection;
using WorkFit.SharedKernel.RegisterModuleServices;

namespace WorkFit.CodeReview;

public sealed class RegisterCodeReviewModuleServices : IRegisterModuleServices
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.Configure<Infrastructure.Options.CodeReviewOptions>(configuration.GetSection("CodeReview"));

        services.AddDbContext<CodeReviewDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddHttpClient("CodeReviewGitHub", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddHttpClient("CodeReviewMistral", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(120);
        });

        services.AddScoped<ICodeReviewRepository, CodeReviewRepository>();
        services.AddScoped<IGitHubCodeReviewService, GitHubCodeReviewService>();
        services.AddScoped<IGitHubAppAuthenticationService, GitHubAppAuthenticationService>();
        services.AddScoped<ICodeReviewAgentService, CodeReviewAgentService>();
        services.AddScoped<ICodeReviewReviewerService, CodeReviewReviewerService>();
        services.AddScoped<ICodeReviewWorkflowService, CodeReviewWorkflowService>();
        services.AddScoped<IReviewTaskGitHub, ReviewTaskGitHubService>();
        services.AddHostedService<CodeReviewDatabaseInitializer>();

        services.AddMediatorHandlers<ModuleMarker>();
    }
}
