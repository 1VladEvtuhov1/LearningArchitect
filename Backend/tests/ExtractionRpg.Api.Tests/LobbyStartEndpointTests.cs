using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExtractionRpg.Api.Auth;
using ExtractionRpg.Api.Lobbies;
using ExtractionRpg.Api.Matches;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExtractionRpg.Api.Tests;

public class LobbyStartEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;

    public LobbyStartEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", string.Empty);
        }).CreateClient();
    }

    [Fact]
    public async Task Start_requires_authentication()
    {
        using HttpResponseMessage response = await _client.PostAsync("/api/lobbies/lobby_x/start", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Start_by_non_host_returns_host_only()
    {
        LoginByNameResponse host = await LoginAsync("StartHost");
        LoginByNameResponse guest = await LoginAsync("StartGuest");
        LobbyDetailDto lobby = await CreateLobbyAsync(host, "Start Room", 4);
        await JoinLobbyAsync(guest, lobby.LobbyId);
        await SetReadyAsync(host, lobby.LobbyId, true);
        await SetReadyAsync(guest, lobby.LobbyId, true);

        using HttpRequestMessage request = AuthenticatedPost($"/api/lobbies/{lobby.LobbyId}/start", guest.SessionToken);
        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Contains("HOST_ONLY", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Start_when_not_all_ready_returns_player_not_ready()
    {
        LoginByNameResponse host = await LoginAsync("ReadyHostOnly");
        LoginByNameResponse guest = await LoginAsync("ReadyGuestIdle");
        LobbyDetailDto lobby = await CreateLobbyAsync(host, "Ready Gate", 4);
        await JoinLobbyAsync(guest, lobby.LobbyId);
        await SetReadyAsync(host, lobby.LobbyId, true);

        using HttpRequestMessage request = AuthenticatedPost($"/api/lobbies/{lobby.LobbyId}/start", host.SessionToken);
        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("PLAYER_NOT_READY", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Start_when_all_ready_returns_match_and_removes_lobby_from_open_list()
    {
        LoginByNameResponse host = await LoginAsync("HappyHost");
        LoginByNameResponse guest = await LoginAsync("HappyGuest");
        LobbyDetailDto lobby = await CreateLobbyAsync(host, "Happy Path", 4);
        await JoinLobbyAsync(guest, lobby.LobbyId);
        await SetReadyAsync(host, lobby.LobbyId, true);
        await SetReadyAsync(guest, lobby.LobbyId, true);

        using HttpRequestMessage request = AuthenticatedPost($"/api/lobbies/{lobby.LobbyId}/start", host.SessionToken);
        using HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        MatchStartResponse? start = await response.Content.ReadFromJsonAsync<MatchStartResponse>(JsonOptions);
        Assert.NotNull(start);
        Assert.StartsWith("match_", start.MatchId);
        Assert.Contains(start.MatchId, start.ConnectUrl);

        using HttpResponseMessage listResponse = await _client.GetAsync("/api/lobbies");
        listResponse.EnsureSuccessStatusCode();
        LobbyListResponse? list = await listResponse.Content.ReadFromJsonAsync<LobbyListResponse>(JsonOptions);
        Assert.NotNull(list);
        Assert.DoesNotContain(list.Items, item => item.LobbyId == lobby.LobbyId);
    }

    [Fact]
    public async Task Start_twice_returns_match_already_started()
    {
        LoginByNameResponse host = await LoginAsync("TwiceHost");
        LobbyDetailDto lobby = await CreateLobbyAsync(host, "Solo Start", 4);
        await SetReadyAsync(host, lobby.LobbyId, true);
        await StartLobbyAsync(host, lobby.LobbyId);

        using HttpRequestMessage request = AuthenticatedPost($"/api/lobbies/{lobby.LobbyId}/start", host.SessionToken);
        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("MATCH_ALREADY_STARTED", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Solo_host_can_start_when_ready()
    {
        LoginByNameResponse host = await LoginAsync("SoloHost");
        LobbyDetailDto lobby = await CreateLobbyAsync(host, "Solo", 4);
        await SetReadyAsync(host, lobby.LobbyId, true);

        MatchStartResponse start = await StartLobbyAsync(host, lobby.LobbyId);
        Assert.StartsWith("match_", start.MatchId);
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

    private async Task<MatchStartResponse> StartLobbyAsync(LoginByNameResponse user, string lobbyId)
    {
        using HttpRequestMessage request = AuthenticatedPost($"/api/lobbies/{lobbyId}/start", user.SessionToken);
        using HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MatchStartResponse>(JsonOptions))!;
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
