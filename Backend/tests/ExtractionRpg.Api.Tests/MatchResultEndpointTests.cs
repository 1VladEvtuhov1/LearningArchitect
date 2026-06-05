using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ExtractionRpg.Api.Auth;
using ExtractionRpg.Api.Lobbies;
using ExtractionRpg.Api.Matches;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExtractionRpg.Api.Tests;

public class MatchResultEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly HttpClient _client;

    public MatchResultEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Postgres", string.Empty);
        }).CreateClient();
    }

    [Fact]
    public async Task SubmitResult_requires_authentication()
    {
        using HttpResponseMessage response = await _client.PostAsJsonAsync(
            "/api/matches/match_x/result",
            new SubmitMatchResultRequest(MatchOutcomes.Extracted, 42f));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SubmitResult_for_unknown_match_returns_not_found()
    {
        LoginByNameResponse user = await LoginAsync("GhostRunner");
        using HttpRequestMessage request = AuthenticatedPost(
            "/api/matches/match_missing/result",
            user.SessionToken,
            new SubmitMatchResultRequest(MatchOutcomes.Extracted, 10f));

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("MATCH_NOT_FOUND", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task SubmitResult_by_non_participant_returns_forbidden()
    {
        LoginByNameResponse host = await LoginAsync("ResultHost");
        LoginByNameResponse outsider = await LoginAsync("ResultOutsider");
        MatchStartResponse start = await StartSoloMatchAsync(host);

        using HttpRequestMessage request = AuthenticatedPost(
            $"/api/matches/{start.MatchId}/result",
            outsider.SessionToken,
            new SubmitMatchResultRequest(MatchOutcomes.Extracted, 12f));

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Contains("NOT_MATCH_PARTICIPANT", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task SubmitResult_twice_returns_already_submitted()
    {
        LoginByNameResponse host = await LoginAsync("TwiceRunner");
        MatchStartResponse start = await StartSoloMatchAsync(host);

        await SubmitResultAsync(host, start.MatchId, MatchOutcomes.Extracted, 30f);

        using HttpRequestMessage request = AuthenticatedPost(
            $"/api/matches/{start.MatchId}/result",
            host.SessionToken,
            new SubmitMatchResultRequest(MatchOutcomes.Death, 31f));

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("RESULT_ALREADY_SUBMITTED", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task SubmitResult_with_invalid_outcome_returns_bad_request()
    {
        LoginByNameResponse host = await LoginAsync("BadOutcome");
        MatchStartResponse start = await StartSoloMatchAsync(host);

        using HttpRequestMessage request = AuthenticatedPost(
            $"/api/matches/{start.MatchId}/result",
            host.SessionToken,
            new SubmitMatchResultRequest("Victory", 5f));

        using HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("MATCH_OUTCOME_INVALID", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task SubmitResult_persists_extracted_outcome()
    {
        LoginByNameResponse host = await LoginAsync("Extractor");
        MatchStartResponse start = await StartSoloMatchAsync(host);

        MatchResultResponse result = await SubmitResultAsync(host, start.MatchId, MatchOutcomes.Extracted, 95.5f);

        Assert.Equal(start.MatchId, result.MatchId);
        Assert.Equal(host.UserId, result.UserId);
        Assert.Equal(MatchOutcomes.Extracted, result.Outcome);
        Assert.Equal(95.5f, result.ElapsedSeconds);
    }

    private async Task<MatchStartResponse> StartSoloMatchAsync(LoginByNameResponse host)
    {
        LobbyDetailDto lobby = await CreateLobbyAsync(host, "Result Room", 4);
        await SetReadyAsync(host, lobby.LobbyId, true);
        return await StartLobbyAsync(host, lobby.LobbyId);
    }

    private async Task<MatchResultResponse> SubmitResultAsync(
        LoginByNameResponse user,
        string matchId,
        string outcome,
        float elapsedSeconds)
    {
        using HttpRequestMessage request = AuthenticatedPost(
            $"/api/matches/{matchId}/result",
            user.SessionToken,
            new SubmitMatchResultRequest(outcome, elapsedSeconds));

        using HttpResponseMessage response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MatchResultResponse>(JsonOptions))!;
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
