//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using System;
//namespace WorkFit.ProjectManagement.Infrastructure.Data.Seed; 
//public static class ProjectManagementSeedExtensions
//{
//    public static async Task SeedProjectManagementAsync(this IServiceProvider services)
//    {
//        using var scope = services.CreateScope();

//        var db = scope.ServiceProvider.GetRequiredService<WorkFitProjectDbContext>();

//        await db.Database.MigrateAsync();

//        await ProjectManagementSeeder.SeedAsync(db);
//    }
//}