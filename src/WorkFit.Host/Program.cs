using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Security.Claims;
using WorkFit.Host.ExtentionMethods;
using WorkFit.Host.GlobalExceptionHandler;
using WorkFit.ProjectManagement.Infrastructure.Data.Seed;

namespace WorkFit.Host
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // get all assemblies that start with "WorkFit." in the base directory
            var assembliesToScan = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "WorkFit.*.dll")
                .Select(Assembly.LoadFrom)
                .ToArray();
            // Register Modules Services
            builder.Services.RegisterModules(builder.Configuration, assembliesToScan);
            builder.Services.AddMediatR(cfg =>
            {
                foreach (var assembly in assembliesToScan)
                {
                    cfg.RegisterServicesFromAssembly(assembly);
                }
            });

            builder.Services.AddFastEndpoints(o => o.Assemblies = assembliesToScan)
                             .SwaggerDocument(o =>
                             {
                                 o.DocumentSettings = s =>
                                 {
                                     s.Title = "WorkFit API";
                                     s.Version = "v1";
                                 };
                             });
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),


                        RoleClaimType = ClaimTypes.Role
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<ExceptionHandler>();

            var app = builder.Build();

            app.UseExceptionHandler();


            // Configure the HTTP request pipeline.
            app.UseHttpsRedirection();


            app.UseAuthentication();
            app.UseAuthorization();

            app.UseFastEndpoints()
               .UseSwaggerGen();

            await app.Services.SeedProjectManagementAsync();

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