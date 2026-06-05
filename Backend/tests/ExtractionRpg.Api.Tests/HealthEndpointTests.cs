using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ExtractionRpg.Api.Tests;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", string.Empty);
        }).CreateClient();
    }

    [Fact]
    public async Task Root_returns_service_identity()
    {
        using HttpResponseMessage response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("ExtractionRpg.Api", body);
    }

    [Fact]
    public async Task Health_returns_success()
    {
        using HttpResponseMessage response = await _client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Ready_without_postgres_config_returns_degraded_payload()
    {
        using HttpResponseMessage response = await _client.GetAsync("/health/ready");
        response.EnsureSuccessStatusCode();
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Degraded", body);
    }
}
