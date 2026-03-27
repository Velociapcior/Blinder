using OpenIddict.Abstractions;

namespace Blinder.IdentityServer.Infrastructure.Auth;

/// <summary>
/// Hosted service that seeds the required OAuth2 client application on startup.
///
/// CRITICAL: Without this seeder, every token request returns <c>invalid_client</c>.
/// The <c>blinder-mobile</c> client is a public client (no secret) used for mobile ROPC
/// and refresh token flows. Authorization code grant is seeded for future social login.
/// </summary>
public sealed class OpenIddictSeeder(IServiceProvider provider) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope = provider.CreateAsyncScope();
        var manager = scope.ServiceProvider
            .GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("blinder-mobile", ct) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "blinder-mobile",
                // Public client — no secret (mobile ROPC, no client secret exchange)
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Revocation,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                }
            }, ct);
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
