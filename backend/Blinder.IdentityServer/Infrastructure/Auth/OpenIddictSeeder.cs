using OpenIddict.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blinder.IdentityServer.Infrastructure.Auth;

/// <summary>
/// Hosted service that seeds the required OAuth2 client application and scopes on startup.
///
/// CRITICAL #1: Without this seeder, every token request returns <c>invalid_client</c>.
/// CRITICAL #2: The "api" scope (with audience "blinder-api") MUST be seeded first.
///              Without it, issued access tokens will not have "blinder-api" in the <c>aud</c> claim,
///              and <c>Blinder.Api</c>'s AddAudiences("blinder-api") validation will reject every token.
/// </summary>
public sealed class OpenIddictSeeder(IServiceProvider provider) : IHostedService
{
    private static readonly string[] RequiredClientPermissions =
    [
        OpenIddictConstants.Permissions.Endpoints.Token,
        OpenIddictConstants.Permissions.Endpoints.Revocation,
        OpenIddictConstants.Permissions.GrantTypes.Password,
        OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
        OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
        OpenIddictConstants.Permissions.ResponseTypes.Code,
        OpenIddictConstants.Permissions.Scopes.Email,
        OpenIddictConstants.Permissions.Prefixes.Scope + "api",
    ];

    public async Task StartAsync(CancellationToken ct)
    {
        await using var scope = provider.CreateAsyncScope();

        // ── Seed the "api" scope with audience "blinder-api" ─────────────────
        // This puts "blinder-api" in the aud claim of issued access tokens.
        // Blinder.Api's AddAudiences("blinder-api") validates this claim.
        var scopeManager = scope.ServiceProvider
            .GetRequiredService<IOpenIddictScopeManager>();

        var apiScope = await scopeManager.FindByNameAsync("api", ct);
        if (apiScope is null)
        {
            await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
            {
                Name = "api",
                Resources = { "blinder-api" }  // sets aud = "blinder-api" on issued tokens
            }, ct);
        }
        else
        {
            var scopeDescriptor = new OpenIddictScopeDescriptor();
            await scopeManager.PopulateAsync(scopeDescriptor, apiScope, ct);

            if (!scopeDescriptor.Resources.Contains("blinder-api", StringComparer.Ordinal))
            {
                scopeDescriptor.Resources.Add("blinder-api");
                await scopeManager.UpdateAsync(apiScope, scopeDescriptor, ct);
            }
        }

        // ── Seed the "blinder-mobile" public client application ───────────────
        var appManager = scope.ServiceProvider
            .GetRequiredService<IOpenIddictApplicationManager>();

        var mobileClient = await appManager.FindByClientIdAsync("blinder-mobile", ct);
        if (mobileClient is null)
        {
            await appManager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "blinder-mobile",
                ClientType = OpenIddictConstants.ClientTypes.Public, // no secret — mobile ROPC
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.Endpoints.Revocation,
                    OpenIddictConstants.Permissions.GrantTypes.Password,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Prefixes.Scope + "api",
                }
            }, ct);
        }
        else
        {
            var appDescriptor = new OpenIddictApplicationDescriptor();
            await appManager.PopulateAsync(appDescriptor, mobileClient, ct);

            var needsClientTypeUpdate = !string.Equals(
                appDescriptor.ClientType,
                OpenIddictConstants.ClientTypes.Public,
                StringComparison.Ordinal);

            var missingPermissions = RequiredClientPermissions
                .Where(permission => !appDescriptor.Permissions.Contains(permission, StringComparer.Ordinal))
                .ToArray();

            if (needsClientTypeUpdate || missingPermissions.Length > 0)
            {
                appDescriptor.ClientType = OpenIddictConstants.ClientTypes.Public;

                foreach (var permission in missingPermissions)
                {
                    appDescriptor.Permissions.Add(permission);
                }

                await appManager.UpdateAsync(mobileClient, appDescriptor, ct);
            }
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
