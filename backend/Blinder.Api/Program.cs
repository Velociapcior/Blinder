using Blinder.Api.Infrastructure.Data;
using Blinder.Api.Models;
using Coravel;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    // ARCH-4: Never call MigrateAsync() on startup.
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

    // --------------------------------------------------------------------
    // Coravel background jobs — scheduler and queue registered here.
    // Actual IInvocable jobs are wired in later stories.
    // --------------------------------------------------------------------
    builder.Services.AddScheduler();
    builder.Services.AddQueue();

    // --------------------------------------------------------------------
    // Packages registered as references only in this story;
    // full configuration deferred to later stories:
    //   JWT Bearer auth   → Story 2.1
    //   S3 client factory → Story 3.2
    //   Firebase / APNs   → Story 5.4
    // --------------------------------------------------------------------

    var app = builder.Build();

    // --------------------------------------------------------------------
    // Middleware pipeline — order matters.
    // UseExceptionHandler MUST come before routing to catch all errors.
    // --------------------------------------------------------------------
    app.UseExceptionHandler();
    app.UseStatusCodePages();

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

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
