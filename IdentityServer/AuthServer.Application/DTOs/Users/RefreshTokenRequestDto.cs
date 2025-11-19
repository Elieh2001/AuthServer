using System.ComponentModel.DataAnnotations;

namespace AuthServer.Application.DTOs.Users;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;

    [Required]
    public string ClientId { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
