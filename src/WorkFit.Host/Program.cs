using FastEndpoints;
using FastEndpoints.Swagger;
using WorkFit.Infrastructure;
using WorkFit.Organizations;

namespace WorkFit.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register Modules Services
            builder.Services.AddInfrastructure();
            builder.Services.AddOrganizationModule(builder.Configuration);

            builder.Services.AddFastEndpoints()
                             .SwaggerDocument(o =>
                             {
                                 o.DocumentSettings = s =>
                                 {
                                     s.Title = "WorkFit API";
                                     s.Version = "v1";
                                 };
                             });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();

            app.UseFastEndpoints()
               .UseSwaggerGen();

            app.Run();
        }
    }
}