using ExtractionRpg.Api.Auth;
using ExtractionRpg.Api.Common;
using ExtractionRpg.Api.Lobbies;
using ExtractionRpg.Api.Matches;
using ExtractionRpg.Api.Profile;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AuthSessionOptions>(
    builder.Configuration.GetSection(AuthSessionOptions.SectionName));
builder.Services.Configure<MatchConnectOptions>(
    builder.Configuration.GetSection(MatchConnectOptions.SectionName));

builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<ISessionStore, InMemorySessionStore>();
builder.Services.AddSingleton<ILobbyRepository, InMemoryLobbyRepository>();
builder.Services.AddSingleton<IMatchRepository, InMemoryMatchRepository>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<LobbyService>();
builder.Services.AddSingleton<MatchService>();

builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    options.AddPolicy("UnityWebGL", policy =>
    {
        string[] origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:8080", "http://localhost:8081" };

        policy.WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

string? postgresConnection = builder.Configuration.GetConnectionString("Postgres");
IHealthChecksBuilder healthChecks = builder.Services.AddHealthChecks()
    .AddCheck("live", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "live" });

if (!string.IsNullOrWhiteSpace(postgresConnection))
{
    healthChecks.AddNpgSql(postgresConnection, name: "postgres", tags: new[] { "ready" });
}

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors("UnityWebGL");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => Results.Ok(new { service = "ExtractionRpg.Api", status = "running" }));

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live"),
});

if (!string.IsNullOrWhiteSpace(postgresConnection))
{
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = registration => registration.Tags.Contains("ready"),
    });
}
else
{
    app.MapGet("/health/ready", () => Results.Json(new
    {
        status = "Degraded",
        reason = "ConnectionStrings:Postgres is not configured.",
    }));
}

app.MapAuthEndpoints();
app.MapProfileEndpoints();
app.MapLobbyEndpoints();
app.MapMatchEndpoints();

app.Run();

public partial class Program;
