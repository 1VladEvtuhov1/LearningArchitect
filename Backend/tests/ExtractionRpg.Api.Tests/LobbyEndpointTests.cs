using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExtractionRpg.Api.Auth;
using ExtractionRpg.Api.Lobbies;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExtractionRpg.Api.Tests;

public class LobbyEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;

    public LobbyEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", string.Empty);
        }).CreateClient();
    }

    [Fact]
    public async Task CreateLobby_requires_authentication()
    {
        using HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/lobbies",
            new CreateLobbyRequest("Forest Run", 4));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateLobby_and_list_returns_open_lobby()
    {
        LoginByNameResponse host = await LoginAsync("HostPlayer");
        LobbyDetailDto created = await CreateLobbyAsync(host, "Forest Run", 4);

        Assert.StartsWith("lobby_", created.LobbyId);
        Assert.Equal("Forest Run", created.Name);
        Assert.Equal(host.UserId, created.HostUserId);
        Assert.Equal("Preparing", created.State);
        Assert.Single(created.Players);
        Assert.True(created.Players[0].IsHost);
        Assert.False(created.Players[0].IsReady);

        using HttpResponseMessage listResponse = await _client.GetAsync("/api/lobbies");
        listResponse.EnsureSuccessStatusCode();

        LobbyListResponse? list = await listResponse.Content.ReadFromJsonAsync<LobbyListResponse>(JsonOptions);
        Assert.NotNull(list);
        Assert.Contains(list.Items, item => item.LobbyId == created.LobbyId && item.PlayerCount == 1);
    }

    [Fact]
    public async Task JoinLobby_adds_second_player()
    {
        LoginByNameResponse host = await LoginAsync("JoinHost");
        LoginByNameResponse guest = await LoginAsync("JoinGuest");
        LobbyDetailDto created = await CreateLobbyAsync(host, "Duo Run", 4);

        LobbyDetailDto joined = await JoinLobbyAsync(guest, created.LobbyId);
        Assert.Equal(2, joined.Players.Count);
        Assert.Contains(joined.Players, player => player.UserId == guest.UserId && !player.IsHost);
    }

    [Fact]
    public async Task JoinLobby_when_full_returns_lobby_full()
    {
        LoginByNameResponse host = await LoginAsync("FullHost");
        LoginByNameResponse guest = await LoginAsync("FullGuest");
        LoginByNameResponse extra = await LoginAsync("FullExtra");
        LobbyDetailDto created = await CreateLobbyAsync(host, "Tiny Room", 2);

        await JoinLobbyAsync(guest, created.LobbyId);

        using HttpRequestMessage request = AuthenticatedPost($"/api/lobbies/{created.LobbyId}/join", extra.SessionToken);
        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("LOBBY_FULL", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task SetReady_updates_player_flag()
    {
        LoginByNameResponse host = await LoginAsync("ReadyHost");
        LobbyDetailDto created = await CreateLobbyAsync(host, "Ready Check", 4);

        LobbyDetailDto ready = await SetReadyAsync(host, created.LobbyId, true);
        LobbyPlayerDto hostPlayer = Assert.Single(ready.Players, player => player.UserId == host.UserId);
        Assert.True(hostPlayer.IsReady);
    }

    [Fact]
    public async Task SetReady_when_not_member_returns_forbidden()
    {
        LoginByNameResponse host = await LoginAsync("MemberHost");
        LoginByNameResponse outsider = await LoginAsync("Outsider");
        LobbyDetailDto created = await CreateLobbyAsync(host, "Private Room", 4);

        using HttpRequestMessage request = AuthenticatedPost(
            $"/api/lobbies/{created.LobbyId}/ready",
            outsider.SessionToken,
            new SetReadyRequest(true));

        using HttpResponseMessage response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Contains("NOT_LOBBY_MEMBER", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task JoinLobby_unknown_id_returns_not_found()
    {
        LoginByNameResponse user = await LoginAsync("LostPlayer");

        using HttpRequestMessage request = AuthenticatedPost("/api/lobbies/lobby_missing/join", user.SessionToken);
        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("LOBBY_NOT_FOUND", await response.Content.ReadAsStringAsync());
    }

    private async Task<LoginByNameResponse> LoginAsync(string username)
    {
        using HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/auth/login-by-name",
            new LoginByNameRequest(username));

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LoginByNameResponse>(JsonOptions))!;
    }

    private async Task<LobbyDetailDto> CreateLobbyAsync(LoginByNameResponse user, string name, int maxPlayers)
    {
        using HttpRequestMessage request = AuthenticatedPost(
            "/api/lobbies",
            user.SessionToken,
            new CreateLobbyRequest(name, maxPlayers));

        using HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LobbyDetailDto>(JsonOptions))!;
    }

    private async Task<LobbyDetailDto> JoinLobbyAsync(LoginByNameResponse user, string lobbyId)
    {
        using HttpRequestMessage request = AuthenticatedPost($"/api/lobbies/{lobbyId}/join", user.SessionToken);
        using HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LobbyDetailDto>(JsonOptions))!;
    }

    private async Task<LobbyDetailDto> SetReadyAsync(LoginByNameResponse user, string lobbyId, bool isReady)
    {
        using HttpRequestMessage request = AuthenticatedPost(
            $"/api/lobbies/{lobbyId}/ready",
            user.SessionToken,
            new SetReadyRequest(isReady));

        using HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<LobbyDetailDto>(JsonOptions))!;
    }

    private static HttpRequestMessage AuthenticatedPost(string url, string token, object? body = null)
    {
        HttpRequestMessage request = new(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
            request.Content = JsonContent.Create(body);
        return request;
    }
}
