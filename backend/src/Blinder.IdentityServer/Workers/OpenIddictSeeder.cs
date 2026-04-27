using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Blinder.IdentityServer.Workers;

public sealed class OpenIddictSeeder(
    IServiceProvider serviceProvider,
    IHostEnvironment environment,
    IConfiguration configuration) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var forceSeeding = configuration.GetValue<bool>("SEED_OPENIDDICT_CLIENTS");
        if (!environment.IsDevelopment() && !forceSeeding)
        {
            return;
        }

        await using var scope = serviceProvider.CreateAsyncScope();

        var appManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
        var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

        await SeedApiScopeAsync(scopeManager, cancellationToken);
        await SeedMobileClientAsync(appManager, cancellationToken);
        await SeedAdminClientAsync(appManager, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task SeedApiScopeAsync(IOpenIddictScopeManager manager, CancellationToken ct)
    {
        var existing = await manager.FindByNameAsync("blinder-api", ct);
        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = "blinder-api",
            DisplayName = "Blinder API",
            Resources = { "blinder-api" }
        };

        if (existing is null)
        {
            await manager.CreateAsync(descriptor, ct);
        }
        else
        {
            await manager.UpdateAsync(existing, descriptor, ct);
        }
    }

    private static async Task SeedMobileClientAsync(IOpenIddictApplicationManager manager, CancellationToken ct)
    {
        var existing = await manager.FindByClientIdAsync("blinder-mobile", ct);
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "blinder-mobile",
            DisplayName = "Blinder Mobile App",
            ClientType = ClientTypes.Public,
            RedirectUris = { new Uri("com.blinder.app://auth/callback") },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Revocation,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + Scopes.OpenId,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + Scopes.OfflineAccess,
                Permissions.Prefixes.Scope + "blinder-api",
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange,
            }
        };

        if (existing is null)
        {
            await manager.CreateAsync(descriptor, ct);
        }
        else
        {
            await manager.UpdateAsync(existing, descriptor, ct);
        }
    }

    private static async Task SeedAdminClientAsync(IOpenIddictApplicationManager manager, CancellationToken ct)
    {
        var existing = await manager.FindByClientIdAsync("blinder-admin", ct);
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "blinder-admin",
            DisplayName = "Blinder Admin Panel",
            ClientType = ClientTypes.Confidential,
            ClientSecret = "blinder-admin-dev-secret",
            RedirectUris = { new Uri("https://admin.localhost/signin-oidc") },
            PostLogoutRedirectUris = { new Uri("https://admin.localhost/signout-callback-oidc") },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + Scopes.OpenId,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Prefixes.Scope + "blinder-api",
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange,
            }
        };

        if (existing is null)
        {
            await manager.CreateAsync(descriptor, ct);
        }
        else
        {
            await manager.UpdateAsync(existing, descriptor, ct);
        }
    }
}
