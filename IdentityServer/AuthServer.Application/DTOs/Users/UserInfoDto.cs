namespace AuthServer.Application.DTOs.Users;

public class UserInfoDto
{
    public Guid UserId { get; set; }
    public Guid? TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool EmailVerified { get; set; }
    public bool IsSystemAdmin { get; set; }
    public string? LegacyUserId { get; set; }
}
