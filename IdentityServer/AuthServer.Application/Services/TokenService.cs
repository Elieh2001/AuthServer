using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthServer.Application.Interfaces;
using AuthServer.Domain.Entities.Tokens;
using AuthServer.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthServer.Application.Services;


public class TokenService : ITokenService
{
    #region Members

    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    #endregion

    #region Constructors

    public TokenService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    #endregion

    #region Public Methods

    public async Task<string> GenerateAccessTokenAsync(Guid userId, Guid? tenantId, Guid applicationId, Dictionary<string, string> additionalClaims = null)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        var application = await _unitOfWork.Applications.GetByIdAsync(applicationId);
        if (application == null)
            throw new Exception("Application not found");

        // Get the legacy user ID mapping if it exists
        var userMapping = await _unitOfWork.ApplicationUserMappings
            .FirstOrDefaultAsync(m => m.UserId == userId && m.ApplicationId == applicationId);

        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("app_id", applicationId.ToString()),
                new Claim("security_stamp", user.SecurityStamp.ToString()),
                new Claim("is_system_admin", user.IsSystemAdmin.ToString().ToLower())
            };

        // Add legacy user ID if mapping exists
        if (userMapping != null && !string.IsNullOrEmpty(userMapping.LegacyUserId))
        {
            claims.Add(new Claim("legacy_user_id", userMapping.LegacyUserId));
        }

        if (tenantId.HasValue)
        {
            claims.Add(new Claim("tenant_id", tenantId.Value.ToString()));
        }

        if (!string.IsNullOrEmpty(user.Roles))
        {
            // Split roles by comma and add each as a separate claim
            var roles = user.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role.Trim()));
            }
        }

        if (!string.IsNullOrEmpty(user.FirstName))
            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));

        if (!string.IsNullOrEmpty(user.LastName))
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

        // Add additional claims
        if (additionalClaims != null)
        {
            foreach (var claim in additionalClaims)
            {
                claims.Add(new Claim(claim.Key, claim.Value));
            }
        }

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

        // Ensure the key is exactly as configured
        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT SecretKey is not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expirationMinutes = application.AccessTokenLifetimeSeconds.HasValue
            ? application.AccessTokenLifetimeSeconds.Value / 60
            : int.Parse(jwtSettings["AccessTokenExpirationMinutes"]);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId, Guid? tenantId, Guid applicationId, string ipAddress, string userAgent)
    {
        // Validate application exists
        var application = await _unitOfWork.Applications.GetByIdAsync(applicationId);
        if (application == null)
            throw new InvalidOperationException($"Application with ID {applicationId} not found");

        // Validate user exists
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expirationDays = application.RefreshTokenLifetimeSeconds.HasValue
            ? application.RefreshTokenLifetimeSeconds.Value / 86400
            : int.Parse(jwtSettings["RefreshTokenExpirationDays"]);

        // Generate JWT refresh token
        var tokenId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, tokenId.ToString()),
            new Claim("token_type", "refresh_token"),
            new Claim("app_id", applicationId.ToString()),
            new Claim("security_stamp", user.SecurityStamp.ToString())
        };

        if (tenantId.HasValue)
        {
            claims.Add(new Claim("tenant_id", tenantId.Value.ToString()));
        }

        var secretKey = jwtSettings["SecretKey"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(expirationDays),
            signingCredentials: creds
        );

        var refreshTokenValue = new JwtSecurityTokenHandler().WriteToken(token);
        var tokenHash = HashToken(refreshTokenValue);

        var refreshToken = new RefreshToken
        {
            Id = tokenId,
            UserId = userId,
            TenantId = tenantId,
            ApplicationId = applicationId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedByIp = ipAddress,
            UserAgent = userAgent,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return refreshTokenValue;
    }
    public async Task<bool> ValidateRefreshTokenAsync(string token, Guid userId)
    {
        try
        {
            // Validate JWT structure and signature
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(int.Parse(jwtSettings["ClockSkewMinutes"]))
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;

            // Verify it's a refresh token
            var tokenType = jwtToken.Claims.FirstOrDefault(c => c.Type == "token_type")?.Value;
            if (tokenType != "refresh_token")
                return false;

            // Verify user ID matches
            var tokenUserId = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (tokenUserId != userId.ToString())
                return false;

            // Check if token exists in database and is not revoked
            var tokenHash = HashToken(token);
            var refreshToken = await _unitOfWork.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.UserId == userId);

            if (refreshToken == null || !refreshToken.IsActive())
                return false;

            // Check if user's security stamp changed (password changed, etc.)
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                await RevokeRefreshTokenAsync(token, "User not found", null);
                return false;
            }

            var tokenSecurityStamp = jwtToken.Claims.FirstOrDefault(c => c.Type == "security_stamp")?.Value;
            if (tokenSecurityStamp != user.SecurityStamp.ToString())
            {
                await RevokeRefreshTokenAsync(token, "Security stamp changed", null);
                return false;
            }

            return true;
        }
        catch (SecurityTokenException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> RevokeRefreshTokenAsync(string token, string reason, string ipAddress)
    {
        var tokenHash = HashToken(token);
        var refreshToken = await _unitOfWork.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        if (refreshToken == null || refreshToken.IsRevoked)
            return false;

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedReason = reason;
        refreshToken.RevokedByIp = ipAddress;

        _unitOfWork.RefreshTokens.Update(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public string GenerateVerificationToken()
    {
        return GenerateSecureToken();
    }

    public string HashToken(string token)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }

    private string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }

    #endregion
}
