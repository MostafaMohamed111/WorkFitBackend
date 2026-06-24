using FastEndpoints;
using FastEndpoints.Swagger;
using WorkFit.Identity;
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
            builder.Services.AddIdentityModule(builder.Configuration);
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



            // seed roles
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var roleManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Identity.Domain.Entities.WorkFitRole>>();
                string[] roleNames = { "SuperAdmin", "Admin", "OrganizationOwner", "Employee", "TeamLeader" };
                foreach (var roleName in roleNames)
                {
                    var roleExists = roleManager.RoleExistsAsync(roleName).Result;
                    if (!roleExists)
                    {
                        var result = roleManager.CreateAsync(new Identity.Domain.Entities.WorkFitRole(roleName)).Result;
                        if (!result.Succeeded)
                        {
                            throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                    }
                }
            }

            app.Run();



        }
    }
}