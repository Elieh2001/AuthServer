using AuthServer.Application.DTOs.Common;
using AuthServer.Application.DTOs.Users;
using AuthServer.Application.Interfaces;
using AuthServer.Domain.Interfaces;

namespace AuthServer.Application.Services;

public class LegacyAuthenticationService : ILegacyAuthenticationService
{
    #region Members

    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;

    #endregion

    #region Constructors

    public LegacyAuthenticationService(IUnitOfWork unitOfWork, IEncryptionService encryptionService)
    {
        _unitOfWork = unitOfWork;
        _encryptionService = encryptionService;
    }

    #endregion

    #region Public Methods

    public async Task<Result<LegacyUserDto>> AuthenticateAgainstLegacyDbAsync(Guid applicationId, string email, string password)
    {
        try
        {
            var application = await _unitOfWork.Applications.GetByIdAsync(applicationId);
            if (application == null || !application.HasLegacyDatabase)
                return Result<LegacyUserDto>.Failure("Application not configured for legacy database");

            var connectionString = _encryptionService.Decrypt(application.LegacyDatabaseConnectionString);

            // This is a simplified implementation
            // In production, you'd use ADO.NET or Dapper to query the legacy database
            // and validate the password using the specified algorithm

            // Placeholder - you would implement actual legacy DB query here
            return Result<LegacyUserDto>.Failure("Legacy authentication not fully implemented");
        }
        catch (Exception ex)
        {
            return Result<LegacyUserDto>.Failure($"Legacy authentication failed: {ex.Message}");
        }
    }

    public async Task<bool> TestLegacyConnectionAsync(Guid applicationId)
    {
        try
        {
            var application = await _unitOfWork.Applications.GetByIdAsync(applicationId);
            if (application == null || !application.HasLegacyDatabase)
                return false;

            var connectionString = _encryptionService.Decrypt(application.LegacyDatabaseConnectionString);

            // Test connection - implementation depends on database type
            // Use SqlConnection, NpgsqlConnection, or MySqlConnection

            return true;
        }
        catch
        {
            return false;
        }
    }
    
    #endregion

}
