namespace Kyvo.Application.Services.RefreshTokenHasher;

public interface IRefreshTokenHasher
{
    string Hash(string refreshToken);
}
