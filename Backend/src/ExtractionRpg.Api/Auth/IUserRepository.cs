namespace ExtractionRpg.Api.Auth;

public interface IUserRepository
{
    UserAccount GetOrCreate(string username);
    bool TryGetById(string userId, out UserAccount? user);
}
