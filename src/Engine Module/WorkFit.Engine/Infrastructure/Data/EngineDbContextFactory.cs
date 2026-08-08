using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WorkFit.Engine.Infrastructure.Data;

public sealed class EngineDbContextFactory : IDesignTimeDbContextFactory<EngineDbContext>
{
    public EngineDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found.");

        var options = new DbContextOptionsBuilder<EngineDbContext>();
        options.UseSqlServer(connectionString);

        return new EngineDbContext(options.Options);
    }
}
