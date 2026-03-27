using Blinder.Api.Infrastructure.Data;
using Blinder.Api.Models;
using Blinder.Api.Services.Registration;
using Coravel;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Serilog;

// Bootstrap logger — captures startup errors before host logger is configured.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // --------------------------------------------------------------------
    // Logging — Serilog replaces all default Microsoft logging providers.
    // No Console.WriteLine, no ILogger<T> from the default provider.
    // PII must never appear in structured log properties (rule #7).
    // --------------------------------------------------------------------
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    // --------------------------------------------------------------------
    // MVC / RazorPages / SignalR
    // --------------------------------------------------------------------
    builder.Services.AddControllers();
    builder.Services.AddRazorPages();
    builder.Services.AddSignalR();

    // --------------------------------------------------------------------
    // RFC 7807 Problem Details — all 4xx/5xx use AddProblemDetails.
    // Error type URIs are defined in Errors/AppErrors.cs only.
    // --------------------------------------------------------------------
    builder.Services.AddProblemDetails();

    // --------------------------------------------------------------------
    // FluentValidation — MUST register from assembly scanning.
    // Never register validators manually.
    // --------------------------------------------------------------------
    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // --------------------------------------------------------------------
    // EF Core / Identity
    // Connection string wiring and migration pipeline come in Story 1.4.
    // Registered here so DI graph is complete for compilation.
    // Production deployments apply idempotent SQL manually.
    // Development uses a guarded startup migration helper for local convenience.
    // --------------------------------------------------------------------
    builder.Services.AddDbContextPool<AppDbContext>(options =>
        options
            // All table/column names snake_case — required by ARCH convention (rule #1).
            // UseSnakeCaseNamingConvention is on DbContextOptionsBuilder, not ModelBuilder.
            .UseNpgsql(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention());

    builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

    // IEmailSender is required by the scaffolded Identity Razor Pages.
    // Full email delivery is wired in a later story; no-op keeps the DI graph valid now.
    builder.Services.AddTransient<IEmailSender, NoOpEmailSender>();

    // Shared registration service — single source of Identity-backed user creation (rule #17).
    builder.Services.AddScoped<IRegistrationService, RegistrationService>();

    // --------------------------------------------------------------------
    // Coravel background jobs — scheduler and queue registered here.
    // Actual IInvocable jobs are wired in later stories.
    // --------------------------------------------------------------------
    builder.Services.AddScheduler();
    builder.Services.AddQueue();

    // --------------------------------------------------------------------
    // OpenIddict remote token validation (AC10 — Story 2.0)
    // Tokens are issued by Blinder.IdentityServer; validated here via OIDC discovery.
    // JwtBearer is NOT used — OpenIddict validation replaces it entirely.
    // Signing keys are fetched and cached from .well-known/openid-configuration.
    // --------------------------------------------------------------------
    builder.Services.AddOpenIddict()
        .AddValidation(options =>
        {
            options.SetIssuer(builder.Configuration["Auth:IdentityServerUrl"]!);
            options.UseSystemNetHttp();   // fetches .well-known/openid-configuration, caches JWKS
            options.UseAspNetCore();
        });

    builder.Services.AddAuthentication(
        OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

    // --------------------------------------------------------------------
    // Packages registered as references only in this story;
    // full configuration deferred to later stories:
    //   S3 client factory → Story 3.2
    //   Firebase / APNs   → Story 5.4
    // --------------------------------------------------------------------

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        await app.MigrateDatabaseAsync();
    }

    // --------------------------------------------------------------------
    // Middleware pipeline — order matters.
    // UseExceptionHandler MUST come before routing to catch all errors.
    // --------------------------------------------------------------------
    app.UseExceptionHandler();
    app.UseStatusCodePages();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    // Lightweight readiness endpoint for local smoke testing.
    app.MapGet("/health", () => Results.Ok(new
    {
        status = "ok",
        utc = DateTimeOffset.UtcNow,
    }));

    app.MapControllers();
    app.MapRazorPages();
    // SignalR hub mapping deferred to Story 5.1:
    // app.MapHub<ChatHub>("/hubs/chat");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Blinder.Api terminated unexpectedly during startup");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Placeholder email sender satisfying the IEmailSender contract required by Identity scaffolding.
/// In development: silently discards all outbound emails.
/// In non-development environments: logs a warning per discarded email so
/// the misconfiguration is visible in monitoring before it reaches production.
/// Replace with a real implementation (e.g. MailKit/SendGrid) in a later story.
/// </summary>
internal sealed class NoOpEmailSender(
    IWebHostEnvironment env,
    ILogger<NoOpEmailSender> logger) : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (!env.IsDevelopment())
            logger.LogWarning(
                "NoOpEmailSender discarded email to {Email} (subject: {Subject}). " +
                "Configure a real IEmailSender before deploying to production.",
                email, subject);

        return Task.CompletedTask;
    }
}
