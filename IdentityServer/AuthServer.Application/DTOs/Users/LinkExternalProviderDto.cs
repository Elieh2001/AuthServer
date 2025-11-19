using System.ComponentModel.DataAnnotations;

namespace AuthServer.Application.DTOs.Users;

public class LinkExternalProviderDto
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string ProviderUserId { get; set; } = string.Empty;

    public string ProviderEmail { get; set; } = string.Empty;
    public string ProviderDisplayName { get; set; } = string.Empty;
}
