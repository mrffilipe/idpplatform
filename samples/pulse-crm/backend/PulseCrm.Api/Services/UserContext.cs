using System.Security.Claims;

namespace PulseCrm.Api.Services;

public sealed class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
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

    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value
        ?? User?.FindFirst("email")?.Value;

    public IReadOnlyList<string> TenantRoles =>
        User?.FindAll("trole").Select(c => c.Value).Distinct().ToList() ?? [];

    public IReadOnlyList<string> PlatformRoles =>
        User?.FindAll("prole").Select(c => c.Value).Distinct().ToList() ?? [];

    public IReadOnlyDictionary<string, string> AllClaims =>
        User?.Claims
            .GroupBy(c => c.Type)
            .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(c => c.Value)))
        ?? new Dictionary<string, string>();

    private static Guid? ParseGuid(string? value) =>
        Guid.TryParse(value, out var id) ? id : null;
}
