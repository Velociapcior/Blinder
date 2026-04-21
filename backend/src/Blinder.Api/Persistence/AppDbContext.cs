using Microsoft.EntityFrameworkCore;

namespace Blinder.Api.Persistence;

/// <summary>
/// Represents the API persistence boundary and its schema-scoped EF Core model.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    /// <summary>
    /// Applies the app schema default and discovers entity configurations for this boundary.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(AppPersistenceDefaults.Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}

internal static class AppPersistenceDefaults
{
    internal const string ConnectionStringName = "DefaultConnection";
    internal const string MigrationsHistoryTable = "__EFMigrationsHistory";
    internal const string Schema = "app";
    internal const string SchemaMarkerTable = "schema_markers";
}