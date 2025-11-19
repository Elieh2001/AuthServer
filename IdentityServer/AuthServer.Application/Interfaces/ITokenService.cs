namespace AuthServer.Application.Interfaces;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(Guid userId, Guid tenantId, Guid applicationId, Dictionary<string, string> additionalClaims = null);
    Task<string> GenerateRefreshTokenAsync(Guid userId, Guid tenantId, Guid applicationId, string ipAddress, string userAgent);
    Task<bool> ValidateRefreshTokenAsync(string token, Guid userId);
    Task<bool> RevokeRefreshTokenAsync(string token, string reason, string ipAddress);
    string GenerateVerificationToken();
    string HashToken(string token);
}
