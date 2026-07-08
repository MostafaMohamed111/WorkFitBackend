using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WorkFit.Skills.Infrastructure.Data;

public sealed class WorkFitSkillsDbContextFactory
    : IDesignTimeDbContextFactory<WorkFitSkillsDbContext>
{
    public WorkFitSkillsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found.");

        var options = new DbContextOptionsBuilder<WorkFitSkillsDbContext>();

        options.UseSqlServer(connectionString);

        return new WorkFitSkillsDbContext(options.Options);
    }
}