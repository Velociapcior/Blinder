using System.IO;
using Blinder.Api.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var dataProtectionKeysDirectory = new DirectoryInfo(
    Path.Combine(builder.Environment.ContentRootPath, "dataprotection-keys"));
var apiConnectionString = GetRequiredConnectionString(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        apiConnectionString,
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
            AppPersistenceDefaults.MigrationsHistoryTable,
            AppPersistenceDefaults.Schema));
});
builder.Services
    .AddDataProtection()
    .SetApplicationName("Blinder")
    .PersistKeysToFileSystem(dataProtectionKeysDirectory);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services
    .AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer(
            builder.Configuration["OpenIddict:Issuer"]
            ?? "http://localhost:5041/");
        options.AddAudiences("blinder-api");
        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");

app.Run();

// Fail fast when the API host is missing its schema-scoped database connection string.
static string GetRequiredConnectionString(IConfiguration configuration) =>
    configuration.GetConnectionString(AppPersistenceDefaults.ConnectionStringName)
    ?? throw new InvalidOperationException(
        $"Connection string '{AppPersistenceDefaults.ConnectionStringName}' was not configured for Blinder.Api.");
