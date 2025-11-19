using System.ComponentModel.DataAnnotations;

namespace AuthServer.Application.DTOs.Tenants;

public class UpdateTenantDto
{
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    public string SubscriptionPlan { get; set; } = string.Empty;
    public int? PasswordMinLength { get; set; }
    public bool? PasswordRequireUppercase { get; set; }
    public bool? PasswordRequireLowercase { get; set; }
    public bool? PasswordRequireDigit { get; set; }
    public bool? PasswordRequireSpecialChar { get; set; }
    public int? SessionTimeoutMinutes { get; set; }
    public int? MaxFailedLoginAttempts { get; set; }
    public int? AccountLockoutDurationMinutes { get; set; }
    public string LogoUrl { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
}
