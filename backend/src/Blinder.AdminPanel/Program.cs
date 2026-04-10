using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

var dataProtectionKeysDirectory = new DirectoryInfo(
    Path.Combine(builder.Environment.ContentRootPath, "dataprotection-keys"));

builder.Services.AddRazorPages();
builder.Services.AddHealthChecks();
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

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
