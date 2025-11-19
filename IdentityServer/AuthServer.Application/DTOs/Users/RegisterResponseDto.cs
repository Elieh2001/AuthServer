namespace AuthServer.Application.DTOs.Users;

public class RegisterResponseDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool EmailVerificationRequired { get; set; }
}
