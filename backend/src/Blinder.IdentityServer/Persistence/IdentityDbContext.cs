using Microsoft.EntityFrameworkCore;

namespace Blinder.IdentityServer.Persistence;

/// <summary>
/// Represents the identity persistence boundary and its schema-scoped EF Core model.
/// </summary>
public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : DbContext(options)
{
    /// <summary>
    /// Applies the identity schema default and discovers entity configurations for this boundary.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(IdentityPersistenceDefaults.Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}

internal static class IdentityPersistenceDefaults
{
    internal const string ConnectionStringName = "DefaultConnection";
    internal const string MigrationsHistoryTable = "__EFMigrationsHistory";
    internal const string Schema = "identity";
    internal const string SchemaMarkerTable = "schema_markers";
}