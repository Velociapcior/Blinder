using System.Net;
using Blinder.AdminPanel;
using Blinder.Api;
using Blinder.IdentityServer;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Blinder.IntegrationTests;

public sealed class ApiHealthEndpointTests(WebApplicationFactory<ApiAssemblyMarker> factory)
    : IClassFixture<WebApplicationFactory<ApiAssemblyMarker>>
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
    private static HttpClient CreateHttpsClient(WebApplicationFactory<ApiAssemblyMarker> appFactory) =>
        appFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
        });
}

public sealed class IdentityServerHealthEndpointTests(WebApplicationFactory<IdentityServerAssemblyMarker> factory)
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

public sealed class AdminPanelHealthEndpointTests(WebApplicationFactory<AdminPanelAssemblyMarker> factory)
    : IClassFixture<WebApplicationFactory<AdminPanelAssemblyMarker>>
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
    private static HttpClient CreateHttpsClient(WebApplicationFactory<AdminPanelAssemblyMarker> appFactory) =>
        appFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
        });
}