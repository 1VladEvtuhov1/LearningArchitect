namespace ExtractionRpg.Api.Auth;

public interface ISessionStore
{
    SessionRecord Create(string userId, TimeSpan lifetime);
    bool TryGetValid(string token, DateTimeOffset now, out SessionRecord? session);
}
