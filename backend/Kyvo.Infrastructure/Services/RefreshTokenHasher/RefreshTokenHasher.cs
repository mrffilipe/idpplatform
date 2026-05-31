using System.Security.Cryptography;
using System.Text;
using Kyvo.Application.Services.RefreshTokenHasher;

namespace Kyvo.Infrastructure.Services.RefreshTokenHasher;

public sealed class RefreshTokenHasher : IRefreshTokenHasher
{
    public string Hash(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }
}
