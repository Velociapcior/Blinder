using Microsoft.EntityFrameworkCore;

namespace Blinder.IdentityServer.Infrastructure.Data;

/// <summary>
/// Minimal DbContext that manages ONLY the four OpenIddict tables:
/// OpenIddictApplications, OpenIddictAuthorizations, OpenIddictTokens, OpenIddictScopes.
///
/// ApplicationUser / Identity tables remain in Blinder.Api's AppDbContext.
/// Both contexts share the same PostgreSQL database via the same connection string
/// but maintain separate EF migration histories under their respective projects.
/// </summary>
public class OpenIddictDbContext(DbContextOptions<OpenIddictDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Maps the four OpenIddict tables — no Identity tables here.
        builder.UseOpenIddict();
        builder.HasDefaultSchema("public");
    }
}
