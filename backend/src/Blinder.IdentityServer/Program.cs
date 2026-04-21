using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Blinder.IdentityServer.Persistence;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    options.UseNpgsql(
        identityConnectionString,
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
            IdentityPersistenceDefaults.MigrationsHistoryTable,
            IdentityPersistenceDefaults.Schema));
});
builder.Services
    .AddDataProtection()
    .SetApplicationName("Blinder")
    .PersistKeysToFileSystem(dataProtectionKeysDirectory);
builder.Services.AddSingleton(identityKeyMaterial);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseHttpsRedirection();
}

app.MapHealthChecks("/health");

app.Run();

return;

// Fail fast when the identity host is missing its schema-scoped database connection string.
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
