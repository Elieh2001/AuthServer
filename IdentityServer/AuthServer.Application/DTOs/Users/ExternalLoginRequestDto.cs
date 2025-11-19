
using System.ComponentModel.DataAnnotations;

namespace AuthServer.Application.DTOs.Users;

public class ExternalLoginRequestDto
{
    [Required]
    public string Provider { get; set; } = string.Empty; // Google, Apple, LinkedIn

    [Required]
    public string ProviderToken { get; set; } = string.Empty;

    [Required]
    public string ClientId { get; set; } = string.Empty;

    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}
