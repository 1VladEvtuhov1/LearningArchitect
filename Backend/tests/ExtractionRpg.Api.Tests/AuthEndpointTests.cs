using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExtractionRpg.Api.Auth;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExtractionRpg.Api.Tests;

public class AuthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;

    public AuthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", string.Empty);
        }).CreateClient();
    }

    [Fact]
    public async Task LoginByName_valid_username_returns_session()
    {
        using HttpResponseMessage response = await PostLoginAsync("AsdaRunner");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        LoginByNameResponse? body = await response.Content.ReadFromJsonAsync<LoginByNameResponse>(JsonOptions);
        Assert.NotNull(body);
        Assert.StartsWith("u_", body.UserId);
        Assert.Equal("AsdaRunner", body.Username);
        Assert.False(string.IsNullOrWhiteSpace(body.SessionToken));
        Assert.True(body.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task LoginByName_invalid_username_returns_structured_error()
    {
        using HttpResponseMessage response = await PostLoginAsync("ab");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        string json = await response.Content.ReadAsStringAsync();
        Assert.Contains("USERNAME_INVALID", json);
        Assert.Contains("3-16 characters", json);
    }

    [Fact]
    public async Task LoginByName_same_username_reuses_user_id()
    {
        LoginByNameResponse? first = await ReadLoginAsync("ReuseMe_1");
        LoginByNameResponse? second = await ReadLoginAsync("ReuseMe_1");

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal(first.UserId, second.UserId);
        Assert.NotEqual(first.SessionToken, second.SessionToken);
    }

    [Fact]
    public async Task ProfileMe_without_token_returns_unauthorized()
    {
        using HttpResponseMessage response = await _client.GetAsync("/api/profile/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string json = await response.Content.ReadAsStringAsync();
        Assert.Contains("UNAUTHORIZED", json);
    }

    [Fact]
    public async Task ProfileMe_with_valid_token_returns_profile()
    {
        LoginByNameResponse? login = await ReadLoginAsync("ProfileUser");
        Assert.NotNull(login);

        using HttpRequestMessage request = new(HttpMethod.Get, "/api/profile/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", login.SessionToken);

        using HttpResponseMessage response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ProfileResponse? profile = await response.Content.ReadFromJsonAsync<ProfileResponse>(JsonOptions);
        Assert.NotNull(profile);
        Assert.Equal(login.UserId, profile.UserId);
        Assert.Equal("ProfileUser", profile.Username);
        Assert.True(profile.LastSeenAt >= profile.CreatedAt);
    }

    [Fact]
    public async Task ProfileMe_with_invalid_token_returns_unauthorized()
    {
        using HttpRequestMessage request = new(HttpMethod.Get, "/api/profile/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-real-token");

        using HttpResponseMessage response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        string json = await response.Content.ReadAsStringAsync();
        Assert.Contains("SESSION_EXPIRED", json);
    }

    private Task<HttpResponseMessage> PostLoginAsync(string username) =>
        _client.PostAsJsonAsync("/api/auth/login-by-name", new LoginByNameRequest(username));

    private async Task<LoginByNameResponse?> ReadLoginAsync(string username)
    {
        using HttpResponseMessage response = await PostLoginAsync(username);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginByNameResponse>(JsonOptions);
    }
}
