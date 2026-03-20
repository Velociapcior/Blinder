using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Blinder.Api.Infrastructure.Data;

/// <summary>
/// Host helpers for development-time infrastructure bootstrap.
/// </summary>
public static class HostExtensions
{
    /// <summary>
    /// Creates the configured development database when it does not exist and then applies pending EF Core migrations.
    /// Production environments must continue to use the checked-in idempotent SQL deployment script.
    /// </summary>
    /// <param name="host">The application host.</param>
    /// <param name="cancellationToken">Cancellation token for startup work.</param>
    public static async Task MigrateDatabaseAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(host);

        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var environment = services.GetRequiredService<IHostEnvironment>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(HostExtensions));

        if (!environment.IsDevelopment())
        {
            logger.LogDebug("Skipping automatic database migration outside the Development environment.");
            return;
        }

        // Read the raw connection string from IConfiguration, not from context.Database.GetConnectionString().
        // In Npgsql v7+, GetConnectionString() returns the NpgsqlDataSource's ConnectionString property,
        // which strips the password for security. IConfiguration always has the original value with password.
        var configuration = services.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        await EnsureDatabaseExistsAsync(connectionString, logger, cancellationToken);

        var context = services.GetRequiredService<AppDbContext>();
        logger.LogInformation("Applying pending EF Core migrations to the development database.");

        try
        {
            await context.Database.MigrateAsync(cancellationToken);
        }
        catch (PostgresException exception) when (exception.SqlState == "42P07")
        {
            // Local/dev safety net: if schema objects already exist but migration history is missing,
            // allow the app to start instead of crash-looping. Shared environments must use SQL scripts.
            logger.LogWarning(
                exception,
                "Detected existing relation(s) during development startup migration. " +
                "Skipping automatic migration to avoid crash loop.");
        }
    }

    /// <summary>
    /// Ensures the configured PostgreSQL database exists before EF Core attempts to migrate it.
    /// </summary>
    /// <param name="connectionString">The application connection string.</param>
    /// <param name="logger">Logger used for startup diagnostics.</param>
    /// <param name="cancellationToken">Cancellation token for the database calls.</param>
    private static async Task EnsureDatabaseExistsAsync(
        string connectionString,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);

        if (string.IsNullOrWhiteSpace(connectionStringBuilder.Database))
        {
            throw new InvalidOperationException("The default connection string must include a database name.");
        }

        var databaseName = connectionStringBuilder.Database;
        var adminConnectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "postgres",
            Pooling = false,
        };

        await using var connection = new NpgsqlConnection(adminConnectionStringBuilder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var existsCommand = new NpgsqlCommand(
            "SELECT 1 FROM pg_database WHERE datname = @databaseName;",
            connection);
        existsCommand.Parameters.AddWithValue("databaseName", databaseName);

        var databaseExists = await existsCommand.ExecuteScalarAsync(cancellationToken) is not null;

        if (databaseExists)
        {
            return;
        }

        logger.LogInformation("Creating development database {DatabaseName}.", databaseName);

        var escapedDatabaseName = databaseName.Replace("\"", "\"\"");
        await using var createDatabaseCommand = new NpgsqlCommand(
            $"CREATE DATABASE \"{escapedDatabaseName}\";",
            connection);
        await createDatabaseCommand.ExecuteNonQueryAsync(cancellationToken);
    }
}