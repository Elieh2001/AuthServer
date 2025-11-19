using AuthServer.Application.DTOs.Users;
using AuthServer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Api.Controllers;

[ApiController]
[Route("api/Authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IAuthenticationService authenticationService,
        ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user with email and password
    /// </summary>
    [HttpPost("Login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var result = await _authenticationService.LoginAsync(request);

        if (!result.IsSuccess)
        {
            var error = result.Errors?.FirstOrDefault() ?? "Login failed";
            _logger.LogWarning("Login failed for email: {Email}. Reason: {Error}", request.Email, error);
            return Unauthorized(new { error });
        }

        _logger.LogInformation("Login successful for email: {Email}", request.Email);
        return Ok(result.Data);
    }

    /// <summary>
    /// Authenticate user with external provider (Google, Microsoft, etc.)
    /// </summary>
    [HttpPost("ExternalLogin")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginRequestDto request)
    {
        _logger.LogInformation("External login attempt for provider: {Provider}", request.Provider);

        var result = await _authenticationService.ExternalLoginAsync(request);

        if (!result.IsSuccess)
        {
            var error = result.Errors?.FirstOrDefault() ?? "External login failed";
            _logger.LogWarning("External login failed for provider: {Provider}. Reason: {Error}",
                request.Provider, error);
            return Unauthorized(new { error });
        }

        _logger.LogInformation("External login successful for provider: {Provider}", request.Provider);
        return Ok(result.Data);
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", request.Email);

        var result = await _authenticationService.RegisterAsync(request);

        if (!result.IsSuccess)
        {
            var error = result.Errors?.FirstOrDefault() ?? "Registration failed";
            _logger.LogWarning("Registration failed for email: {Email}. Reason: {Error}",
                request.Email, error);
            return BadRequest(new { error });
        }

        _logger.LogInformation("Registration successful for email: {Email}", request.Email);
        return CreatedAtAction(nameof(Register), result.Data);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("RefreshToken")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        _logger.LogInformation("Token refresh attempt");

        var result = await _authenticationService.RefreshTokenAsync(request);

        if (!result.IsSuccess)
        {
            var error = result.Errors?.FirstOrDefault() ?? "Token refresh failed";
            _logger.LogWarning("Token refresh failed. Reason: {Error}", error);
            return Unauthorized(new { error });
        }

        _logger.LogInformation("Token refresh successful");
        return Ok(result.Data);
    }

    /// <summary>
    /// Logout user and revoke refresh token
    /// </summary>
    [HttpPost("Logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogInformation("Logout attempt for user: {UserId}", userId);

        var result = await _authenticationService.LogoutAsync(userId, request.RefreshToken);

        if (!result.IsSuccess)
        {
            var error = result.Errors?.FirstOrDefault() ?? "Logout failed";
            _logger.LogWarning("Logout failed for user: {UserId}. Reason: {Error}", userId, error);
            return BadRequest(new { error });
        }

        _logger.LogInformation("Logout successful for user: {UserId}", userId);
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Revoke all refresh tokens for the current user
    /// </summary>
    [HttpPost("RevokeAllTokens")]
    [Authorize]
    public async Task<IActionResult> RevokeAllTokens()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogInformation("Revoke all tokens for user: {UserId}", userId);

        var result = await _authenticationService.RevokeAllTokensAsync(userId);

        if (!result.IsSuccess)
        {
            var error = result.Errors?.FirstOrDefault() ?? "Revoke all tokens failed";
            _logger.LogWarning("Revoke all tokens failed for user: {UserId}. Reason: {Error}",
                userId, error);
            return BadRequest(new { error });
        }

        _logger.LogInformation("All tokens revoked for user: {UserId}", userId);
        return Ok(new { message = "All tokens revoked successfully" });
    }

    /// <summary>
    /// Verify email address using verification token
    /// </summary>
    [HttpGet("VerifyEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(new { error = "Verification token is required" });
        }

        _logger.LogInformation("Email verification attempt");

        var result = await _authenticationService.VerifyEmailAsync(token);

        if (!result.IsSuccess)
        {
            var error = result.Errors?.FirstOrDefault() ?? "Email verification failed";
            _logger.LogWarning("Email verification failed. Reason: {Error}", error);
            return BadRequest(new { error });
        }

        _logger.LogInformation("Email verification successful");
        return Ok(new { message = "Email verified successfully" });
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    [HttpPost("RequestPasswordReset")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestPasswordReset([FromBody] RequestPasswordResetDto request)
    {
        _logger.LogInformation("Password reset request for email: {Email}", request.Email);

        var result = await _authenticationService.RequestPasswordResetAsync(request.Email, request.TenantId);

        // Always return success to prevent email enumeration
        _logger.LogInformation("Password reset request processed for email: {Email}", request.Email);
        return Ok(new { message = "If the email exists, a password reset link has been sent" });
    }

    /// <summary>
    /// Reset password using reset token
    /// </summary>
    [HttpPost("ResetPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        _logger.LogInformation("Password reset attempt");

        var result = await _authenticationService.ResetPasswordAsync(request);

        if (!result.IsSuccess)
        {
            var error = result.Errors?.FirstOrDefault() ?? "Password reset failed";
            _logger.LogWarning("Password reset failed. Reason: {Error}", error);
            return BadRequest(new { error });
        }

        _logger.LogInformation("Password reset successful");
        return Ok(new { message = "Password reset successfully" });
    }

    /// <summary>
    /// Change password for authenticated user
    /// </summary>
    [HttpPost("ChangePassword")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user token" });
        }

        _logger.LogInformation("Password change attempt for user: {UserId}", userId);

        var result = await _authenticationService.ChangePasswordAsync(userId, request);

        if (!result.IsSuccess)
        {
            var error = result.Errors?.FirstOrDefault() ?? "Password change failed";
            _logger.LogWarning("Password change failed for user: {UserId}. Reason: {Error}",
                userId, error);
            return BadRequest(new { error });
        }

        _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
        return Ok(new { message = "Password changed successfully" });
    }
}

// DTO for password reset request
public class RequestPasswordResetDto
{
    public string Email { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
}
