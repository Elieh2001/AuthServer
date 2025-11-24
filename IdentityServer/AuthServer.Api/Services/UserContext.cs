using System.Security.Claims;

namespace AuthServer.Api.Services;

public interface IUserContext
{
    Guid? GetUserId();
    string? GetUserEmail();
    Guid? GetTenantId();
    Guid? GetApplicationId();
    bool IsSystemAdmin();
    bool IsAuthenticated();
    List<string> GetRoles();
    bool IsInRole(string role);
    ClaimsPrincipal? GetUser();
}

public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserContext> _logger;

    public UserContext(IHttpContextAccessor httpContextAccessor, ILogger<UserContext> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public ClaimsPrincipal? GetUser() => User;

    public bool IsAuthenticated() => User?.Identity?.IsAuthenticated ?? false;

    public Guid? GetUserId()
    {
        var userIdClaim = User?.FindFirst("sub")?.Value ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }

    public string? GetUserEmail()
    {
        return User?.FindFirst("email")?.Value ?? User?.FindFirst(ClaimTypes.Email)?.Value;
    }

    public Guid? GetTenantId()
    {
        var tenantIdClaim = User?.FindFirst("tenant_id")?.Value;
        if (Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return tenantId;
        }
        return null;
    }

    public Guid? GetApplicationId()
    {
        var appIdClaim = User?.FindFirst("app_id")?.Value;
        if (Guid.TryParse(appIdClaim, out var appId))
        {
            return appId;
        }
        return null;
    }

    public bool IsSystemAdmin()
    {
        var isSystemAdminClaim = User?.FindFirst("is_system_admin")?.Value;
        return isSystemAdminClaim?.ToLower() == "true";
    }

    public List<string> GetRoles()
    {
        if (User == null) return new List<string>();

        var roles = User.FindAll("role").Select(c => c.Value).ToList();

        // Also check standard role claim type
        roles.AddRange(User.FindAll(ClaimTypes.Role).Select(c => c.Value));

        return roles.Distinct().ToList();
    }

    public bool IsInRole(string role)
    {
        if (User == null) return false;

        // Check both custom "role" claims and standard ClaimTypes.Role
        var hasRole = User.HasClaim("role", role) || User.HasClaim(ClaimTypes.Role, role);

        _logger.LogDebug("Checking role '{Role}' for user: {Result}. All roles: {Roles}",
            role, hasRole, string.Join(", ", GetRoles()));

        return hasRole;
    }
}
