using Microsoft.EntityFrameworkCore;

using EfIdentityDbContext = Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<
    Blinder.IdentityServer.Persistence.ApplicationUser>;

namespace Blinder.IdentityServer.Persistence;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : EfIdentityDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(IdentityPersistenceDefaults.Schema);

        // Required by ASP.NET Core Identity — must precede custom configurations
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        modelBuilder.UseOpenIddict();
    }
}

internal static class IdentityPersistenceDefaults
{
    internal const string ConnectionStringName = "DefaultConnection";
    internal const string MigrationsHistoryTable = "__EFMigrationsHistory";
    internal const string Schema = "identity";
    internal const string SchemaMarkerTable = "schema_markers";
}
