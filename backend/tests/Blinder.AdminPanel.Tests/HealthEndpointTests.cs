using System.Net;
using Blinder.AdminPanel;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Blinder.AdminPanel.Tests;

public sealed class HealthEndpointTests(WebApplicationFactory<AdminPanelAssemblyMarker> factory)
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