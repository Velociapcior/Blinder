using System.Net.Sockets;
using Blinder.Api.Persistence;
using Blinder.IdentityServer.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Blinder.IntegrationTests;

public sealed class DatabasePermissionSmokeTests
{
    // Requires a live PostgreSQL instance with the bootstrap script already applied.
    // Run: docker compose down -v && docker compose up -d postgres
    // Wait for healthy, then: dotnet ef database update for both projects.
    [Fact]
    [Trait("Category", "Integration")]
    public async Task RuntimeRoles_CannotWriteAcrossSchemas()
    {
        var settings = DatabasePermissionTestSettings.Load();

        if (!await IsDatabaseAvailableAsync(settings.Host, settings.Port))
        {
            // This smoke test validates schema permissions only when an external Postgres instance is reachable.
            return;
        }

        await MigrateIdentitySchemaAsync(settings.IdentityConnectionString);
        await MigrateAppSchemaAsync(settings.ApiConnectionString);
        await AssertSchemaArtifactsExistAsync(settings.AdminConnectionString, "identity");
        await AssertSchemaArtifactsExistAsync(settings.AdminConnectionString, "app");
        await AssertWriteDeniedAsync(
            settings.IdentityConnectionString,
            "INSERT INTO app.schema_markers DEFAULT VALUES;");
        await AssertWriteDeniedAsync(
            settings.ApiConnectionString,
            "INSERT INTO identity.schema_markers DEFAULT VALUES;");
    }

    /// <summary>
    /// Applies the identity schema migrations with the identity-scoped runtime role.
    /// </summary>
    private static async Task MigrateIdentitySchemaAsync(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "identity"));

        await using var context = new IdentityDbContext(optionsBuilder.Options);
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Applies the app schema migrations with the app-scoped runtime role.
    /// </summary>
    private static async Task MigrateAppSchemaAsync(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "app"));

        await using var context = new AppDbContext(optionsBuilder.Options);
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Confirms the isolated schema and migration history artifacts exist after migrations run.
    /// </summary>
    private static async Task AssertSchemaArtifactsExistAsync(string adminConnectionString, string schema)
    {
        await using var connection = new NpgsqlConnection(adminConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                to_regclass(@historyTableName) IS NOT NULL
                AND to_regclass(@markerTableName) IS NOT NULL;
            """;
        command.Parameters.AddWithValue("historyTableName", $"{schema}.\"__EFMigrationsHistory\"");
        command.Parameters.AddWithValue("markerTableName", $"{schema}.schema_markers");

        var result = await command.ExecuteScalarAsync();

        Assert.Equal(true, result);
    }

    /// <summary>
    /// Verifies the runtime role receives PostgreSQL's insufficient privilege error on cross-schema writes.
    /// </summary>
    private static async Task AssertWriteDeniedAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        try
        {
            await connection.OpenAsync();
        }
        catch (NpgsqlException ex)
        {
            throw new InvalidOperationException(
                $"Connection failed — verify role credentials and that the bootstrap script ran. Inner: {ex.Message}", ex);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;

        var exception = await Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync());

        Assert.Equal(PostgresErrorCodes.InsufficientPrivilege, exception.SqlState);
    }

    /// <summary>
    /// Detects whether PostgreSQL is reachable from the host running the test suite.
    /// </summary>
    private static async Task<bool> IsDatabaseAvailableAsync(string host, int port)
    {
        using var tcpClient = new TcpClient();

        try
        {
            await tcpClient.ConnectAsync(host, port);

            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    private sealed record DatabasePermissionTestSettings(
        string Host,
        int Port,
        string AdminConnectionString,
        string ApiConnectionString,
        string IdentityConnectionString)
    {
        private const string DefaultApiPassword = "blinder_api_dev_password";
        private const string DefaultApiUser = "blinder_api";
        private const string DefaultDatabase = "blinder";
        private const string DefaultHost = "localhost";
        private const string DefaultIdentityPassword = "blinder_identity_dev_password";
        private const string DefaultIdentityUser = "blinder_identity";
        private const string DefaultPostgresAdminPassword = "blinder_admin_dev_password";
        private const string DefaultPostgresAdminUser = "blinder_admin";
        private const int DefaultPort = 5432;

        /// <summary>
        /// Builds test connection strings from the same environment variable names used by Docker Compose.
        /// </summary>
        public static DatabasePermissionTestSettings Load()
        {
            var host = GetEnvironmentVariableOrDefault("BLINDER_DB_HOST", DefaultHost);
            var database = GetEnvironmentVariableOrDefault("POSTGRES_DB", DefaultDatabase);
            var portString = GetEnvironmentVariableOrDefault("BLINDER_DB_PORT", DefaultPort.ToString());
            var port = int.TryParse(portString, out var parsedPort) ? parsedPort : DefaultPort;

            return new DatabasePermissionTestSettings(
                host,
                port,
                BuildConnectionString(
                    host,
                    port,
                    database,
                    GetEnvironmentVariableOrDefault("POSTGRES_ADMIN_USER", DefaultPostgresAdminUser),
                    GetEnvironmentVariableOrDefault("POSTGRES_ADMIN_PASSWORD", DefaultPostgresAdminPassword)),
                BuildConnectionString(
                    host,
                    port,
                    database,
                    GetEnvironmentVariableOrDefault("API_DB_USER", DefaultApiUser),
                    GetEnvironmentVariableOrDefault("API_DB_PASSWORD", DefaultApiPassword)),
                BuildConnectionString(
                    host,
                    port,
                    database,
                    GetEnvironmentVariableOrDefault("IDENTITY_DB_USER", DefaultIdentityUser),
                    GetEnvironmentVariableOrDefault("IDENTITY_DB_PASSWORD", DefaultIdentityPassword)));
        }

        /// <summary>
        /// Builds a PostgreSQL connection string for the requested runtime role.
        /// </summary>
        private static string BuildConnectionString(string host, int port, string database, string username, string password) =>
            new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = port,
                Database = database,
                Username = username,
                Password = password,
                SslMode = SslMode.Disable,
            }.ConnectionString;

        /// <summary>
        /// Reads an environment variable while keeping the local development defaults aligned with Compose.
        /// </summary>
        private static string GetEnvironmentVariableOrDefault(string variableName, string defaultValue) =>
            Environment.GetEnvironmentVariable(variableName) ?? defaultValue;
    }
}
