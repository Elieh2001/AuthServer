using AuthServer.Application.DTOs.Common;
using AuthServer.Application.DTOs.System;
using AuthServer.Application.DTOs.Users;
using AuthServer.Application.Interfaces;
using AuthServer.Domain.Entities.Applications;
using AuthServer.Domain.Entities.Tenants;
using AuthServer.Domain.Entities.Users;
using AuthServer.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AuthServer.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IAuditService _auditService;
    private readonly ILegacyAuthenticationService _legacyAuthService;
    private readonly IConfiguration _configuration;

    public AuthenticationService(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IAuditService auditService,
        ILegacyAuthenticationService legacyAuthService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _auditService = auditService;
        _legacyAuthService = legacyAuthService;
        _configuration = configuration;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        try
        {
            // var application = new Domain.Entities.Applications.Application();
            //if (string.IsNullOrEmpty(request.ClientId))
            //{ application = await _unitOfWork.Applications.FirstOrDefaultAsync(a => a.Name == "Default Application"); }

            // Check if the tenant name is passed
            var application = await _unitOfWork.Applications.FirstOrDefaultAsync(a => string.IsNullOrEmpty(request.ClientId) == true ? a.Name == "Default Application" : a.ClientId == request.ClientId);

            if (application == null || !application.IsActive)
            {
                await LogAuditEvent(null, null, request.ClientId, "LoginFailed", false, "Invalid client credentials", request.IpAddress);
                return Result<LoginResponseDto>.Failure("Invalid client credentials");
            }

            // Determine tenant
            Tenant? tenant = null;
            Guid? tenantId = null;

            if (!string.IsNullOrEmpty(request.TenantName))
            {
                tenant = (await _unitOfWork.Tenants.FindAsync(x => x.Name == request.TenantName)).FirstOrDefault();
                tenantId = tenant?.Id;
            }

            // Get user - check for system admin first (null tenantId), then regular user
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsSystemAdmin && u.TenantId == null);

            if (user == null && tenantId.HasValue)
            {
                // If not a system admin, try to find regular user in the specified tenant
                user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.TenantId == tenantId);
            }

            // Handle legacy database authentication
            if (application.HasLegacyDatabase && user == null && tenantId.HasValue)
            {
                var legacyResult = await _legacyAuthService.AuthenticateAgainstLegacyDbAsync(
                    application.Id, request.Email, request.Password);

                if (legacyResult.IsSuccess)
                {
                    // Create or update user in auth server
                    user = await GetOrCreateUserFromLegacy(tenantId.Value, application.Id, legacyResult.Data);
                }
            }

            if (user == null)
            {
                await LogAuditEvent(tenantId, null, request.ClientId, "LoginFailed", false, "User not found", request.IpAddress);
                return Result<LoginResponseDto>.Failure("Invalid email or password");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                await LogAuditEvent(user.TenantId, user.Id, request.ClientId, "LoginFailed", false, "User is inactive", request.IpAddress);
                return Result<LoginResponseDto>.Failure("Account is inactive");
            }

            // Check if user is locked out
            if (user.IsLockedOut())
            {
                await LogAuditEvent(user.TenantId, user.Id, request.ClientId, "LoginFailed", false, "User is locked out", request.IpAddress);
                return Result<LoginResponseDto>.Failure($"Account is locked until {user.LockoutEnd:yyyy-MM-dd HH:mm}");
            }

            // Verify password (skip for legacy users on first login)
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    // Only handle failed login for non-system admins with tenant
                    if (tenant != null)
                    {
                        await HandleFailedLogin(user, tenant);
                    }
                    else
                    {
                        user.AccessFailedCount++;
                        _unitOfWork.Users.Update(user);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    await LogAuditEvent(user.TenantId, user.Id, request.ClientId, "LoginFailed", false, "Invalid password", request.IpAddress);
                    return Result<LoginResponseDto>.Failure("Invalid email or password");
                }
            }

            // Reset failed login attempts
            user.AccessFailedCount = 0;
            user.LastLoginAt = DateTime.UtcNow;
            user.LastLoginIp = request.IpAddress;
            _unitOfWork.Users.Update(user);

            // Generate tokens - use user's tenantId (null for system admin, actual tenant for regular users)
            var accessToken = await _tokenService.GenerateAccessTokenAsync(
                user.Id, user.TenantId, application.Id);

            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(
                user.Id, user.TenantId, application.Id, request.IpAddress, request.UserAgent);

            await _unitOfWork.SaveChangesAsync();

            await LogAuditEvent(user.TenantId, user.Id, request.ClientId, "Login", true, null, request.IpAddress);

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expiresIn = application.AccessTokenLifetimeSeconds ?? (int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15") * 60);

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expiresIn,
                User = new UserInfoDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    TenantId = user.TenantId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmailVerified = user.EmailVerified,
                    IsSystemAdmin = user.IsSystemAdmin
                }
            });
        }
        catch (Exception ex)
        {
            return Result<LoginResponseDto>.Failure($"Login failed: {ex.Message}");
        }
    }

    public async Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        try
        {
            var tokenHash = _tokenService.HashToken(request.RefreshToken);
            var refreshToken = await _unitOfWork.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

            if (refreshToken == null || !refreshToken.IsActive())
            {
                await LogAuditEvent(null, null, request.ClientId, "TokenRefreshFailed", false, "Invalid or expired refresh token", request.IpAddress);
                return Result<LoginResponseDto>.Failure("Invalid or expired refresh token");
            }

            // Load related entities separately
            var user = await _unitOfWork.Users.GetByIdAsync(refreshToken.UserId);
            var application = await _unitOfWork.Applications.GetByIdAsync(refreshToken.ApplicationId);

            refreshToken.User = user;
            refreshToken.Application = application;

            // Verify client
            if (refreshToken.Application.ClientId != request.ClientId)
            {
                await LogAuditEvent(refreshToken.TenantId, refreshToken.UserId, request.ClientId, "TokenRefreshFailed", false, "Client mismatch", request.IpAddress);
                return Result<LoginResponseDto>.Failure("Invalid client");
            }

            // Mark token as used
            refreshToken.IsUsed = true;
            refreshToken.UsedAt = DateTime.UtcNow;
            _unitOfWork.RefreshTokens.Update(refreshToken);

            // Generate new tokens
            var accessToken = await _tokenService.GenerateAccessTokenAsync(
                refreshToken.UserId, refreshToken.TenantId, refreshToken.ApplicationId);

            var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync(
                refreshToken.UserId, refreshToken.TenantId, refreshToken.ApplicationId,
                request.IpAddress, request.UserAgent);

            await _unitOfWork.SaveChangesAsync();

            await LogAuditEvent(refreshToken.TenantId, refreshToken.UserId, request.ClientId, "TokenRefreshed", true, null, request.IpAddress);

            var expiresIn = refreshToken.Application.AccessTokenLifetimeSeconds ??
                (int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "15") * 60);

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = expiresIn,
                User = new UserInfoDto
                {
                    UserId = refreshToken.User.Id,
                    Email = refreshToken.User.Email,
                    TenantId = refreshToken.User.TenantId,
                    FirstName = refreshToken.User.FirstName,
                    LastName = refreshToken.User.LastName,
                    EmailVerified = refreshToken.User.EmailVerified,
                    IsSystemAdmin = refreshToken.User.IsSystemAdmin
                }
            });
        }
        catch (Exception ex)
        {
            return Result<LoginResponseDto>.Failure($"Token refresh failed: {ex.Message}");
        }
    }

    public async Task<Result<bool>> LogoutAsync(Guid userId, string refreshToken)
    {
        try
        {
            await _tokenService.RevokeRefreshTokenAsync(refreshToken, "User logout", null);
            await LogAuditEvent(null, userId, null, "Logout", true, null, null);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Logout failed: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RevokeAllTokensAsync(Guid userId)
    {
        try
        {
            var tokens = await _unitOfWork.RefreshTokens
                .FindAsync(rt => rt.UserId == userId && !rt.IsRevoked);

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedReason = "Revoked all tokens";
            }

            _unitOfWork.RefreshTokens.UpdateRange(tokens);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Revoke tokens failed: {ex.Message}");
        }
    }

    public async Task<Result<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _unitOfWork.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.TenantId == request.TenantId);

            if (existingUser != null)
                return Result<RegisterResponseDto>.Failure("User with this email already exists");

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                TenantId = request.TenantId,
                Email = request.Email,
                PasswordHash = passwordHash,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailVerified = false
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            await LogAuditEvent(request.TenantId, user.Id, request.ClientId, "UserCreated", true, null, null);

            return Result<RegisterResponseDto>.Success(new RegisterResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                EmailVerificationRequired = true
            });
        }
        catch (Exception ex)
        {
            return Result<RegisterResponseDto>.Failure($"Registration failed: {ex.Message}");
        }
    }

    public Task<Result<LoginResponseDto>> ExternalLoginAsync(ExternalLoginRequestDto request)
    {
        throw new NotImplementedException("External login will be implemented with provider-specific logic");
    }

    public Task<Result<bool>> VerifyEmailAsync(string token)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> RequestPasswordResetAsync(string email, Guid tenantId)
    {
        throw new NotImplementedException();
    }

    public Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<bool>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return Result<bool>.Failure("Current password is incorrect");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.SecurityStamp = Guid.NewGuid();

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            // Revoke all refresh tokens
            await RevokeAllTokensAsync(userId);

            await LogAuditEvent(user.TenantId, userId, null, "PasswordChanged", true, null, null);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Password change failed: {ex.Message}");
        }
    }

    private async Task HandleFailedLogin(User user, Tenant tenant)
    {
        user.AccessFailedCount++;

        if (user.AccessFailedCount >= tenant.MaxFailedLoginAttempts)
        {
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(tenant.AccountLockoutDurationMinutes);
            await LogAuditEvent(user.TenantId, user.Id, null, "UserLocked", true, "Too many failed login attempts", null);
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<User> GetOrCreateUserFromLegacy(Guid tenantId, Guid applicationId, LegacyUserDto legacyUser)
    {
        var user = await _unitOfWork.Users
            .FirstOrDefaultAsync(u => u.Email == legacyUser.Email && u.TenantId == tenantId);

        if (user == null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Email = legacyUser.Email,
                FirstName = legacyUser.FirstName,
                LastName = legacyUser.LastName,
                EmailVerified = true
            };

            await _unitOfWork.Users.AddAsync(user);
        }

        // Create or update mapping
        var mapping = await _unitOfWork.ApplicationUserMappings
            .FirstOrDefaultAsync(m => m.UserId == user.Id && m.ApplicationId == applicationId);

        if (mapping == null)
        {
            mapping = new ApplicationUserMapping
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ApplicationId = applicationId,
                LegacyUserId = legacyUser.LegacyUserId
            };
            await _unitOfWork.ApplicationUserMappings.AddAsync(mapping);
        }

        await _unitOfWork.SaveChangesAsync();
        return user;
    }

    private async Task LogAuditEvent(Guid? tenantId, Guid? userId, string? clientId, string eventType, bool success, string? error, string? ipAddress)
    {
        await _auditService.LogEventAsync(new AuditLogDto
        {
            TenantId = tenantId,
            UserId = userId,
            EventType = eventType,
            EventCategory = "Authentication",
            Success = success,
            ErrorMessage = error,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        });
    }
}