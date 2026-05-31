namespace Kyvo.Infrastructure.Persistence;

public static class TenantCacheKeys
{
    public static string BuildIdentifierKey(string identifier)
    {
        return $"tenant:identifier:{identifier.Trim().ToLowerInvariant()}";
    }
}
