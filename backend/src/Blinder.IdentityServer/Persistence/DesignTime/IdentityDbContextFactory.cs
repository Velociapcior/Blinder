using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Blinder.IdentityServer.Persistence.DesignTime;

/// <summary>
/// Builds the identity persistence context for EF Core design-time operations.
/// </summary>
public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    /// <summary>
    /// Creates a configured <see cref="IdentityDbContext"/> without booting the web host.
    /// </summary>
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development;
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString(IdentityPersistenceDefaults.ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{IdentityPersistenceDefaults.ConnectionStringName}' was not configured for design-time migrations.");

        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                IdentityPersistenceDefaults.MigrationsHistoryTable,
                IdentityPersistenceDefaults.Schema));

        return new IdentityDbContext(optionsBuilder.Options);
    }
}