using Blinder.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blinder.Api.Infrastructure.Data;

/// <summary>
/// Primary EF Core database context for Blinder.
/// Extends <c>IdentityDbContext</c> with Guid-keyed users and roles.
/// Conventions:
/// <list type="bullet">
///   <item>All table and column names are snake_case via <c>UseSnakeCaseNamingConvention()</c>.</item>
///   <item>PostGIS extension is declared here so that the first migration emits <c>CREATE EXTENSION IF NOT EXISTS postgis</c> (ARCH-5).</item>
///   <item><c>UseNetTopologySuite()</c> is configured on the Npgsql options builder in <c>Program.cs</c>, not here.</item>
/// </list>
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity tables must be set up before any custom configuration.
        base.OnModelCreating(builder);

        // Declare the PostGIS extension so the initial migration emits
        // CREATE EXTENSION IF NOT EXISTS postgis (required by ARCH-5).
        builder.HasPostgresExtension("postgis");
    }
}
