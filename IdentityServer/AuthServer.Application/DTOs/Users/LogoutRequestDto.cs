using System.ComponentModel.DataAnnotations;

namespace AuthServer.Application.DTOs.Users;

public class LogoutRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
