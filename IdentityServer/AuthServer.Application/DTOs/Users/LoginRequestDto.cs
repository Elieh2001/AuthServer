using System.ComponentModel.DataAnnotations;

namespace AuthServer.Application.DTOs.Users;

public class LoginRequestDto
{
    // Can be either email or username
    [Required]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string TenantName { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public Guid? ApplicationId { get; set; }

    public bool RememberMe { get; set; }

    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;

    // Deprecated - Use EmailOrUsername instead
    [Obsolete("Use EmailOrUsername instead")]
    public string Email
    {
        get => EmailOrUsername;
        set => EmailOrUsername = value;
    }
}
