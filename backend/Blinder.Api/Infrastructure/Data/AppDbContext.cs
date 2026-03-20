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

        const int identityKeyMaxLength = 128;

        // Declare the PostGIS extension so the initial migration emits
        // CREATE EXTENSION IF NOT EXISTS postgis (required by ARCH-5).
        builder.HasPostgresExtension("postgis");

        // EFCore.NamingConventions does not convert explicitly configured table names
        // (those set via ToTable() in IdentityDbContext.OnModelCreating). Override them
        // and index names here to enforce the snake_case requirement consistently.
        builder.Entity<ApplicationUser>(user =>
        {
            user.ToTable("asp_net_users");
            user.Property(item => item.Gender).HasDefaultValue(UserGender.Unspecified);
            user.Property(item => item.IsOnboardingComplete).HasDefaultValue(false);
            user.HasIndex(item => item.InviteLinkId).HasDatabaseName("ix_asp_net_users_invite_link_id");
            user.HasIndex(item => item.NormalizedEmail).HasDatabaseName("email_index");
            user.HasIndex(item => item.NormalizedUserName)
                .IsUnique()
                .HasDatabaseName("user_name_index");
        });

        builder.Entity<IdentityRole<Guid>>(role =>
        {
            role.ToTable("asp_net_roles");
            role.HasIndex(item => item.NormalizedName)
                .IsUnique()
                .HasDatabaseName("role_name_index");
        });

        builder.Entity<IdentityUserRole<Guid>>().ToTable("asp_net_user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("asp_net_user_claims");

        builder.Entity<IdentityUserLogin<Guid>>(login =>
        {
            login.ToTable("asp_net_user_logins");
            login.Property(item => item.LoginProvider).HasMaxLength(identityKeyMaxLength);
            login.Property(item => item.ProviderKey).HasMaxLength(identityKeyMaxLength);
        });

        builder.Entity<IdentityUserToken<Guid>>(token =>
        {
            token.ToTable("asp_net_user_tokens");
            token.Property(item => item.LoginProvider).HasMaxLength(identityKeyMaxLength);
            token.Property(item => item.Name).HasMaxLength(identityKeyMaxLength);
        });

        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("asp_net_role_claims");
    }
}
