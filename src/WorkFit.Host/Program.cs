using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json.Serialization;
using WorkFit.Host.ExtentionMethods;
using WorkFit.Host.GlobalExceptionHandler;

namespace WorkFit.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var assembliesToScan = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "WorkFit.*.dll")
                .Select(Assembly.LoadFrom)
                .ToArray();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularFrontend", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:11428",
                            "https://localhost:11428",
                            "http://localhost:4200",
                            "https://localhost:4200",
                            "http://localhost:4201",
                            "https://localhost:4201"
                          )
                          .SetIsOriginAllowed(_ => true)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            builder.Services.RegisterModules(builder.Configuration, assembliesToScan);
            builder.Services.AddControllers();
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
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            var app = builder.Build();

            app.UseExceptionHandler();

            app.UseRouting();

            // Place CORS after UseRouting and before Auth/Endpoints to handle preflight OPTIONS requests
            app.UseCors("AllowAngularFrontend");

            // Configure the HTTP request pipeline.
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<WorkFit.Host.Compatibility.CompatibilityMiddleware>();

            app.MapControllers();
            app.UseFastEndpoints()
               .UseSwaggerGen();

            // seed roles and demo organization accounts
            //WorkFit.Host.Seeding.DemoDataSeeder.SeedDemoDataAsync(app.Services).GetAwaiter().GetResult();

            app.Run();
        }
    }
}
