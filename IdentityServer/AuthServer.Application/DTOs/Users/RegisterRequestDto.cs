using System.ComponentModel.DataAnnotations;

namespace AuthServer.Application.DTOs.Users;

public class RegisterRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string ClientId { get; set; } = string.Empty;

    public Guid TenantId { get; set; }
}
