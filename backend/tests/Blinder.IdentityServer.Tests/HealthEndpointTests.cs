using System.Net;
using Blinder.IdentityServer;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Blinder.IdentityServer.Tests;

public sealed class HealthEndpointTests(WebApplicationFactory<IdentityServerAssemblyMarker> factory)
    : IClassFixture<WebApplicationFactory<IdentityServerAssemblyMarker>>
{
    [Fact]
    public async Task Health_ReturnsOk()
    {
        using var client = CreateHttpsClient(factory);

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Creates a client that uses HTTPS so the test exercises the endpoint directly.
    /// </summary>
    private static HttpClient CreateHttpsClient(WebApplicationFactory<IdentityServerAssemblyMarker> appFactory) =>
        appFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
        });
}