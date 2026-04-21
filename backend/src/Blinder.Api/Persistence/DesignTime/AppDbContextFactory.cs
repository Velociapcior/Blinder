using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Blinder.Api.Persistence.DesignTime;

/// <summary>
/// Builds the API persistence context for EF Core design-time operations.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// Creates a configured <see cref="AppDbContext"/> without booting the web host.
    /// </summary>
    public AppDbContext CreateDbContext(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development;
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString(AppPersistenceDefaults.ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{AppPersistenceDefaults.ConnectionStringName}' was not configured for design-time migrations.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                AppPersistenceDefaults.MigrationsHistoryTable,
                AppPersistenceDefaults.Schema));

        return new AppDbContext(optionsBuilder.Options);
    }
}