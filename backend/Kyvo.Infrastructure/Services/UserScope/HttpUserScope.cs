using System.Security.Claims;
using Kyvo.Application.Services.UserScope;
using Kyvo.Domain.Constants;
using Microsoft.AspNetCore.Http;

namespace Kyvo.Infrastructure.Services.UserScope;

public sealed class HttpUserScope : IUserScope
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpUserScope(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            var uid = GetGuid("uid");
            return uid != Guid.Empty ? uid : GetGuid("sub");
        }
    }

    public Guid? SessionId => GetNullableGuid("sid");

    public Guid? TenantId => GetNullableGuid("tid");

    public Guid? MembershipId => GetNullableGuid("mid");

    public IReadOnlyList<string> TenantRoles => _httpContextAccessor.HttpContext?.User?
        .FindAll("trole")
        .Select(x => x.Value.Trim().ToLowerInvariant())
        .Where(x => x.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList() ?? [];

    public IReadOnlyList<string> PlatformRoles => _httpContextAccessor.HttpContext?.User?
        .FindAll(PlatformRoleDefaults.ClaimType)
        .Select(x => x.Value.Trim().ToLowerInvariant())
        .Where(x => x.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList() ?? [];

    public bool HasAnyTenantRole(params string[] roleKeys)
    {
        var roles = TenantRoles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return roleKeys.Any(roles.Contains);
    }

    public bool HasAnyPlatformRole(params string[] roleKeys)
    {
        var roles = PlatformRoles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return roleKeys.Any(roles.Contains);
    }

    private Guid GetGuid(string claimName)
    {
        var raw = GetString(claimName);
        return Guid.TryParse(raw, out var value) ? value : Guid.Empty;
    }

    private Guid? GetNullableGuid(string claimName)
    {
        var raw = GetString(claimName);
        return Guid.TryParse(raw, out var value) ? value : null;
    }

    private string? GetString(string claimName)
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue(claimName);
    }
}
