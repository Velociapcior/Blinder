using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Blinder.IdentityServer.Persistence;
using Blinder.IdentityServer.Workers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

var builder = WebApplication.CreateBuilder(args);

var dataProtectionKeysDirectory = new DirectoryInfo(
    Path.Combine(builder.Environment.ContentRootPath, "dataprotection-keys"));
var identityKeysDirectory = new DirectoryInfo(
    Path.Combine(builder.Environment.ContentRootPath, "keys"));
var identityKeysPassword = builder.Configuration["IdentityKeys:Password"] ?? "blinder-local-identity-password";
var identityKeyMaterial = LoadIdentityKeyMaterial(identityKeysDirectory, identityKeysPassword);
var identityConnectionString = GetRequiredConnectionString(builder.Configuration);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddRazorPages();

builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    options.UseNpgsql(
        identityConnectionString,
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
            IdentityPersistenceDefaults.MigrationsHistoryTable,
            IdentityPersistenceDefaults.Schema));
    options.UseOpenIddict();
});

builder.Services
    .AddDataProtection()
    .SetApplicationName("Blinder")
    .PersistKeysToFileSystem(dataProtectionKeysDirectory);

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 10;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

var googleId = builder.Configuration["Auth:Google:ClientId"];
var googleSecret = builder.Configuration["Auth:Google:ClientSecret"];
var hasGoogle = !string.IsNullOrEmpty(googleId) && !string.IsNullOrEmpty(googleSecret);

var facebookId = builder.Configuration["Auth:Facebook:AppId"];
var facebookSecret = builder.Configuration["Auth:Facebook:AppSecret"];
var hasFacebook = !string.IsNullOrEmpty(facebookId) && !string.IsNullOrEmpty(facebookSecret);

// Apple requires a real HTTPS domain for redirect URIs (no localhost).
// The client secret must be a signed JWT derived from an Apple private key.
var appleId = builder.Configuration["Auth:Apple:ClientId"];
var appleSecret = builder.Configuration["Auth:Apple:ClientSecret"];
var hasApple = !string.IsNullOrEmpty(appleId) && !string.IsNullOrEmpty(appleSecret);

var oidcBuilder = builder.Services
    .AddOpenIddict()
    .AddCore(options =>
    {
        options
            .UseEntityFrameworkCore()
            .UseDbContext<IdentityDbContext>();
    });

if (hasGoogle || hasFacebook || hasApple)
{
    oidcBuilder.AddClient(options =>
    {
        options.AllowAuthorizationCodeFlow();

        options.UseAspNetCore()
            .EnableRedirectionEndpointPassthrough();
        options.UseSystemNetHttp();

        var webProviders = options.UseWebProviders();

        if (hasGoogle)
        {
            webProviders.AddGoogle(o => o
                .SetClientId(googleId!)
                .SetClientSecret(googleSecret!)
                .SetRedirectUri("callback/login/google"));
        }

        if (hasFacebook)
        {
            webProviders.AddFacebook(o => o
                .SetClientId(facebookId!)
                .SetClientSecret(facebookSecret!)
                .SetRedirectUri("callback/login/facebook"));
        }

        if (hasApple)
        {
            webProviders.AddApple(o => o
                .SetClientId(appleId!)
                .SetClientSecret(appleSecret!)
                .SetRedirectUri("callback/login/apple")
                .AddScopes("name", "email"));
        }
    });
}

oidcBuilder.AddServer(options =>
    {
        options
            .SetAuthorizationEndpointUris("/connect/authorize")
            .SetTokenEndpointUris("/connect/token")
            .SetUserInfoEndpointUris("/connect/userinfo")
            .SetEndSessionEndpointUris("/connect/endsession")
            .SetIntrospectionEndpointUris("/connect/introspect")
            .SetRevocationEndpointUris("/connect/revoke");

        options
            .AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow();

        options
            .SetAccessTokenLifetime(TimeSpan.FromMinutes(60))
            .SetRefreshTokenLifetime(TimeSpan.FromDays(30));

        options.RequireProofKeyForCodeExchange();

        if (builder.Environment.IsDevelopment())
        {
            options.AddEphemeralEncryptionKey();
            options.AddEphemeralSigningKey();
        }
        else
        {
            options.AddSigningCertificate(identityKeyMaterial.SigningCertificate);
            options.AddEncryptionCertificate(identityKeyMaterial.EncryptionCertificate);
        }

        options
            .UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableEndSessionEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough()
            .EnableStatusCodePagesIntegration();
    });

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddHostedService<OpenIddictSeeder>();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapHealthChecks("/health");

app.Run();

return;

static string GetRequiredConnectionString(IConfiguration configuration) =>
    configuration.GetConnectionString(IdentityPersistenceDefaults.ConnectionStringName)
    ?? throw new InvalidOperationException(
        $"Connection string '{IdentityPersistenceDefaults.ConnectionStringName}' was not configured for Blinder.IdentityServer.");

static IdentityKeyMaterial LoadIdentityKeyMaterial(DirectoryInfo keysDirectory, string password)
{
    if (!keysDirectory.Exists)
    {
        keysDirectory.Create();
    }

    var signingCertificatePath = Path.Combine(keysDirectory.FullName, "signing.pfx");
    var encryptionCertificatePath = Path.Combine(keysDirectory.FullName, "encryption.pfx");

    var signingCertificate = LoadOrCreateCertificate(
        signingCertificatePath,
        password,
        "CN=Blinder Identity Signing");
    var encryptionCertificate = LoadOrCreateCertificate(
        encryptionCertificatePath,
        password,
        "CN=Blinder Identity Encryption");

    return new IdentityKeyMaterial(signingCertificate, encryptionCertificate);
}

static X509Certificate2 LoadOrCreateCertificate(string certificatePath, string password, string subjectName)
{
    if (File.Exists(certificatePath))
    {
        return X509CertificateLoader.LoadPkcs12FromFile(
            certificatePath,
            password,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet,
            Pkcs12LoaderLimits.Defaults);
    }

    using var rsa = RSA.Create(2048);
    var request = new CertificateRequest(
        subjectName,
        rsa,
        HashAlgorithmName.SHA256,
        RSASignaturePadding.Pkcs1);

    request.CertificateExtensions.Add(
        new X509BasicConstraintsExtension(false, false, 0, false));
    request.CertificateExtensions.Add(
        new X509KeyUsageExtension(
            X509KeyUsageFlags.DigitalSignature |
            X509KeyUsageFlags.KeyEncipherment |
            X509KeyUsageFlags.DataEncipherment,
            false));
    request.CertificateExtensions.Add(
        new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

    using var certificate = request.CreateSelfSigned(
        DateTimeOffset.UtcNow.AddDays(-1),
        DateTimeOffset.UtcNow.AddYears(5));
    var certificateBytes = certificate.Export(X509ContentType.Pfx, password);

    File.WriteAllBytes(certificatePath, certificateBytes);

    return X509CertificateLoader.LoadPkcs12(
        certificateBytes,
        password,
        X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet,
        Pkcs12LoaderLimits.Defaults);
}

sealed record IdentityKeyMaterial(X509Certificate2 SigningCertificate, X509Certificate2 EncryptionCertificate);
