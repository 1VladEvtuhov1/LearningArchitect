using Microsoft.AspNetCore.Mvc.Testing;

namespace ExtractionRpg.Api.Tests;

/// <summary>
/// Requires <c>docker compose up -d</c> at repo root and <c>EXTRACTIONRPG_INTEGRATION=1</c>.
/// </summary>
public class PostgresIntegrationTests
{
    [Fact]
    public async Task Ready_with_docker_postgres_returns_healthy()
    {
        if (Environment.GetEnvironmentVariable("EXTRACTIONRPG_INTEGRATION") != "1")
        {
            return;
        }

        await using WebApplicationFactory<Program> factory = new();
        using HttpClient client = factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync("/health/ready");
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", body, StringComparison.OrdinalIgnoreCase);
    }
}
