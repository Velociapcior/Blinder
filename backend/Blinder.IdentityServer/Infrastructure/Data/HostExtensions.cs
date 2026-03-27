using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Blinder.IdentityServer.Infrastructure.Data;

/// <summary>
/// Host helpers for development-time OpenIddict database bootstrap.
/// Only migrates <see cref="OpenIddictDbContext"/> (the 4 OpenIddict tables).
/// Identity / domain tables remain the responsibility of Blinder.Api.
/// </summary>
public static class HostExtensions
{
    /// <summary>
    /// Ensures the configured PostgreSQL database exists and applies pending
    /// OpenIddict EF Core migrations. Development only — production uses the
    /// checked-in idempotent SQL script (migrations/latest-identity.sql).
    /// </summary>
    public static async Task MigrateOpenIddictDatabaseAsync(
        this IHost host,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(host);

        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var environment = services.GetRequiredService<IHostEnvironment>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(HostExtensions));

        // Run only in Development. Testing environment (WebApplicationFactory) skips migration
        // because it uses in-memory databases and has no real PostgreSQL connection.
        if (!environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            logger.LogDebug("Skipping automatic OpenIddict migration (environment: {Environment}).",
                environment.EnvironmentName);
            return;
        }

        var context = services.GetRequiredService<OpenIddictDbContext>();

        // In-memory databases (used in integration tests) don't support migrations.
        if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            logger.LogDebug("Skipping OpenIddict migration for in-memory database.");
            return;
        }

        var configuration = services.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        await EnsureDatabaseExistsAsync(connectionString, logger, cancellationToken);

        logger.LogInformation("Applying pending OpenIddict EF Core migrations.");

        try
        {
            await context.Database.MigrateAsync(cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == "42P07")
        {
            logger.LogWarning(
                ex,
                "Detected existing OpenIddict relation(s) during development startup. " +
                "Skipping automatic migration to avoid crash loop.");
        }
    }

    private static async Task EnsureDatabaseExistsAsync(
        string connectionString,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(builder.Database))
            throw new InvalidOperationException("The connection string must include a database name.");

        var databaseName = builder.Database;
        var adminBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "postgres",
            Pooling = false
        };

        await using var connection = new NpgsqlConnection(adminBuilder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var existsCmd = new NpgsqlCommand(
            "SELECT 1 FROM pg_database WHERE datname = @db;", connection);
        existsCmd.Parameters.AddWithValue("db", databaseName);

        if (await existsCmd.ExecuteScalarAsync(cancellationToken) is not null)
            return;

        logger.LogInformation("Creating development database {Database}.", databaseName);
        var escaped = databaseName.Replace("\"", "\"\"");
        await using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{escaped}\";", connection);
        await createCmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
