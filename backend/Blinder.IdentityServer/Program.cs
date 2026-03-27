using Blinder.Api.Infrastructure.Data;
using Blinder.Api.Models;
using Blinder.IdentityServer.Infrastructure.Auth;
using Blinder.IdentityServer.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

// Bootstrap logger — captures startup errors before host logger is configured.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Logging ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    // ── MVC ───────────────────────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddProblemDetails();

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // ── DbContext 1: AppDbContext (from Blinder.Api) ──────────────────────────
    // Provides Identity UserManager for ROPC credential validation.
    // ApplicationUser / Identity tables are owned by Blinder.Api — IdentityServer is read-only here.
    // Note: AddDbContext (not Pool) — pool complicates integration test overrides; auth server
    // request volume does not justify the extra complexity.
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite())
               .UseSnakeCaseNamingConvention());

    // ── DbContext 2: OpenIddictDbContext ──────────────────────────────────────
    // Manages ONLY the 4 OpenIddict tables. EF migrations run from this project.
    builder.Services.AddDbContext<OpenIddictDbContext>(options =>
        options.UseNpgsql(connectionString)
               .UseSnakeCaseNamingConvention()
               .UseOpenIddict());

    // ── Identity ──────────────────────────────────────────────────────────────
    // CRITICAL: Must use IdentityRole<Guid> — matches Blinder.Api exactly.
    // Identity tables live in AppDbContext (Blinder.Api project).
    builder.Services
        .AddIdentity<ApplicationUser, IdentityRole<Guid>>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    // ── OpenIddict Authorization Server ───────────────────────────────────────
    // See Dev Notes: DI Wiring Pattern for rationale of each option.
    builder.Services.AddOpenIddict()
        .AddCore(options =>
            options.UseEntityFrameworkCore()
                   .UseDbContext<OpenIddictDbContext>())
        .AddServer(options =>
        {
            options.SetTokenEndpointUris("/api/auth/oauth/token");
            options.SetRevocationEndpointUris("/api/auth/oauth/revoke");

            options.AllowPasswordFlow()           // ROPC (AC2)
                   .AllowRefreshTokenFlow()        // Refresh (AC4)
                   .AllowAuthorizationCodeFlow();  // Social login (AC3)

            // Token lifetimes — do NOT change without reading Dev Notes: Token Expiry Rationale.
            // Access tokens: 15 min (short window = revocation not needed per-request).
            // Refresh tokens: 30-day rolling (OpenIddict rotation = automatic replay detection).
            options.SetAccessTokenLifetime(TimeSpan.FromMinutes(15));
            options.SetRefreshTokenLifetime(TimeSpan.FromDays(30));

            // "Testing" is treated like Development for cert/transport config so that
            // WebApplicationFactory integration tests can start without real certificates.
            if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing"))
            {
                // Auto-generated dev certificates stored in user secrets.
                options.AddDevelopmentSigningCertificate();
                options.AddDevelopmentEncryptionCertificate();
                // Allow HTTP in local development (never enable in production).
                options.UseAspNetCore().DisableTransportSecurityRequirement();
            }
            else
            {
                var config = builder.Configuration;
                options.AddSigningCertificate(
                    LoadCert(
                        config["Auth:SigningCertBase64"],
                        config["Auth:SigningCertPassword"],
                        "Auth:SigningCertBase64"));
                options.AddEncryptionCertificate(
                    LoadCert(
                        config["Auth:EncryptionCertBase64"],
                        config["Auth:EncryptionCertPassword"],
                        "Auth:EncryptionCertBase64"));
            }

            // Passthrough mode: controller actions handle grant logic, OpenIddict handles responses.
            // EnableTokenEndpointPassthrough is REQUIRED — without it the controller is never reached.
            options.UseAspNetCore()
                .EnableTokenEndpointPassthrough();
        });

    // ── Rate Limiting (AC11) ──────────────────────────────────────────────────
    // Fixed window: 5 requests per IP per minute on the token endpoint.
    builder.Services.AddRateLimiter(rateLimiterOptions =>
    {
        rateLimiterOptions.AddFixedWindowLimiter("token-endpoint", options =>
        {
            options.PermitLimit = 5;
            options.Window = TimeSpan.FromMinutes(1);
            options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            options.QueueLimit = 0;
        });
    });

    // ── Hosted Services ───────────────────────────────────────────────────────
    // Seeds blinder-mobile client application on startup (AC9).
    // Without this: every token request returns invalid_client.
    builder.Services.AddHostedService<OpenIddictSeeder>();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        await app.MigrateOpenIddictDatabaseAsync();
    }

    // ── Middleware Pipeline ───────────────────────────────────────────────────
    // Order matters: UseAuthentication before UseAuthorization.
    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRateLimiter();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Blinder.IdentityServer terminated unexpectedly during startup");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Loads an RSA X.509 certificate from a base64-encoded string.
/// Used for production signing and encryption certificates.
/// Development uses AddDevelopmentSigningCertificate() instead.
/// </summary>
static X509Certificate2 LoadCert(string? base64, string? password, string keyName)
{
    if (string.IsNullOrWhiteSpace(base64))
    {
        throw new InvalidOperationException($"Configuration value '{keyName}' is required in production.");
    }

    try
    {
        var bytes = Convert.FromBase64String(base64);
        return X509CertificateLoader.LoadPkcs12(bytes, password);
    }
    catch (FormatException ex)
    {
        throw new InvalidOperationException($"Configuration value '{keyName}' is not valid base64.", ex);
    }
    catch (CryptographicException ex)
    {
        throw new InvalidOperationException(
            $"Certificate configured in '{keyName}' could not be loaded. Verify PFX content and password.",
            ex);
    }
}

// Required to make Program accessible to WebApplicationFactory<T> in Blinder.Tests.
public partial class Program { }
