using System.Security.Cryptography;
using System.Text;
using IdPPlatform.Application.Services.RefreshTokenHasher;

namespace IdPPlatform.Infrastructure.Services.RefreshTokenHasher;

public sealed class RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }
}
