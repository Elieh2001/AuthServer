using AuthServer.Application.DTOs.Common;
using AuthServer.Application.DTOs.Users;

namespace AuthServer.Application.Interfaces;

public interface IAuthenticationService
{
    Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request);
    Task<Result<LoginResponseDto>> ExternalLoginAsync(ExternalLoginRequestDto request);
    Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<Result<bool>> LogoutAsync(Guid userId, string refreshToken);
    Task<Result<bool>> RevokeAllTokensAsync(Guid userId);
    Task<Result<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<Result<bool>> VerifyEmailAsync(string token);
    Task<Result<bool>> RequestPasswordResetAsync(string email, Guid tenantId);
    Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<Result<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);

}
