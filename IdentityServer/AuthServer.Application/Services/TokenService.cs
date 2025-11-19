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

    public async Task<string> GenerateAccessTokenAsync(Guid userId, Guid tenantId, Guid applicationId, Dictionary<string, string> additionalClaims = null)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            throw new Exception("User not found");

        var application = await _unitOfWork.Applications.GetByIdAsync(applicationId);
        if (application == null)
            throw new Exception("Application not found");

        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("tenant_id", tenantId.ToString()),
                new Claim("app_id", applicationId.ToString()),
                new Claim("security_stamp", user.SecurityStamp.ToString())
            };

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
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expirationMinutes = application.AccessTokenLifetimeSeconds.HasValue
            ? application.AccessTokenLifetimeSeconds.Value / 60
            : int.Parse(jwtSettings["AccessTokenExpirationMinutes"]);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId, Guid tenantId, Guid applicationId, string ipAddress, string userAgent)
    {
        var refreshTokenValue = GenerateSecureToken();
        var tokenHash = HashToken(refreshTokenValue);

        var application = await _unitOfWork.Applications.GetByIdAsync(applicationId);
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var expirationDays = application.RefreshTokenLifetimeSeconds.HasValue
            ? application.RefreshTokenLifetimeSeconds.Value / 86400
            : int.Parse(jwtSettings["RefreshTokenExpirationDays"]);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            ApplicationId = applicationId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedByIp = ipAddress,
            UserAgent = userAgent
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return refreshTokenValue;
    }

    public async Task<bool> ValidateRefreshTokenAsync(string token, Guid userId)
    {
        var tokenHash = HashToken(token);
        var refreshToken = await _unitOfWork.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.UserId == userId);

        if (refreshToken == null || !refreshToken.IsActive())
            return false;

        // Check if user's security stamp changed (password changed, etc.)
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null || user.SecurityStamp.ToString() != refreshToken.CreatedAt.ToString())
        {
            await RevokeRefreshTokenAsync(token, "Security stamp changed", null);
            return false;
        }

        return true;
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
