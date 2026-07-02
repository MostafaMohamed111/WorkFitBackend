using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WorkFit.Skills.Infrastructure.Data;

public class WorkFitSkillsDbContextFactory : IDesignTimeDbContextFactory<WorkFitSkillsDbContext>
{
    public WorkFitSkillsDbContext CreateDbContext(string[] args)
    {
        // Navigate from WorkFit.Skills up to src, then into WorkFit.Host
        var hostPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "WorkFit.Host"
        );

        var config = new ConfigurationBuilder()
            .SetBasePath(Path.GetFullPath(hostPath))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                $"Connection string 'DefaultConnection' not found. Looked in: {Path.GetFullPath(hostPath)}");

        var optionsBuilder = new DbContextOptionsBuilder<WorkFitSkillsDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new WorkFitSkillsDbContext(optionsBuilder.Options);
    }
}