using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace IdPPlatform.AspNetCore;

public sealed class IdPUserContext : IIdPUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdPUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId => ParseGuid(
        User?.FindFirst("uid")?.Value
        ?? User?.FindFirst("sub")?.Value
        ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value);

    public Guid? TenantId => ParseGuid(User?.FindFirst("tid")?.Value);

    public Guid? MembershipId => ParseGuid(User?.FindFirst("mid")?.Value);

    public string? Email =>
        User?.FindFirst(ClaimTypes.Email)?.Value
        ?? User?.FindFirst("email")?.Value;

    public IReadOnlyList<string> TenantRoles =>
        User?.FindAll("trole").Select(c => c.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        ?? [];

    public IReadOnlyList<string> PlatformRoles =>
        User?.FindAll("prole").Select(c => c.Value).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        ?? [];

    public bool HasTenantRole(params string[] roles) =>
        roles.Length == 0 || roles.Any(r => TenantRoles.Contains(r, StringComparer.OrdinalIgnoreCase));

    public bool HasPlatformRole(params string[] roles) =>
        roles.Length == 0 || roles.Any(r => PlatformRoles.Contains(r, StringComparer.OrdinalIgnoreCase));

    private static Guid? ParseGuid(string? value) =>
        Guid.TryParse(value, out var id) ? id : null;
}
