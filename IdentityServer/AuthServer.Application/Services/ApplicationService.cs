using AuthServer.Application.DTOs.Applications;
using AuthServer.Application.DTOs.Common;
using AuthServer.Application.Interfaces;
using AuthServer.Domain.Interfaces;
using static AuthServer.Domain.Enumerations.Enums;

namespace AuthServer.Application.Services;

public class ApplicationService : IApplicationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;

    public ApplicationService(IUnitOfWork unitOfWork, IEncryptionService encryptionService)
    {
        _unitOfWork = unitOfWork;
        _encryptionService = encryptionService;
    }

    public async Task<Result<ApplicationDto>> CreateApplicationAsync(CreateApplicationDto dto)
    {
        try
        {
            var clientId = GenerateClientId();
            var clientSecret = GenerateClientSecret();
            var clientSecretHash = BCrypt.Net.BCrypt.HashPassword(clientSecret);

            var application = new Domain.Entities.Applications.Application
            {
                Id = Guid.NewGuid(),
                TenantId = dto.TenantId,
                Name = dto.Name,
                Description = dto.Description,
                ClientId = clientId,
                ClientSecretHash = clientSecretHash,
                ApplicationType = Enum.Parse<ApplicationType>(dto.ApplicationType),
                AllowedGrantTypes = dto.AllowedGrantTypes,
                AllowedCorsOrigins = "{ }",
                AllowedScopes = dto.AllowedScopes,
                RedirectUris = dto.RedirectUris,
                PostLogoutRedirectUris = dto.PostLogoutRedirectUris,
                AccessTokenLifetimeSeconds = dto.AccessTokenLifetimeSeconds,
                RefreshTokenLifetimeSeconds = dto.RefreshTokenLifetimeSeconds,
                GoogleEnabled = dto.GoogleEnabled,
                AppleEnabled = dto.AppleEnabled,
                LinkedInEnabled = dto.LinkedInEnabled,
                HasLegacyDatabase = dto.HasLegacyDatabase,
                IsActive = true
            };

            // Google Configuration
            if (dto.GoogleEnabled && !string.IsNullOrEmpty(dto.GoogleClientId))
            {
                application.GoogleClientId = dto.GoogleClientId;
                if (!string.IsNullOrEmpty(dto.GoogleClientSecret))
                {
                    application.GoogleClientSecret = _encryptionService.Encrypt(dto.GoogleClientSecret);
                }
            }

            // Apple Configuration
            if (dto.AppleEnabled && !string.IsNullOrEmpty(dto.AppleClientId))
            {
                application.AppleClientId = dto.AppleClientId;
                application.AppleTeamId = dto.AppleTeamId;
                application.AppleKeyId = dto.AppleKeyId;
                if (!string.IsNullOrEmpty(dto.ApplePrivateKey))
                {
                    application.ApplePrivateKey = _encryptionService.Encrypt(dto.ApplePrivateKey);
                }
            }

            // LinkedIn Configuration
            if (dto.LinkedInEnabled && !string.IsNullOrEmpty(dto.LinkedInClientId))
            {
                application.LinkedInClientId = dto.LinkedInClientId;
                if (!string.IsNullOrEmpty(dto.LinkedInClientSecret))
                {
                    application.LinkedInClientSecret = _encryptionService.Encrypt(dto.LinkedInClientSecret);
                }
            }

            // Legacy Database Configuration
            if (dto.HasLegacyDatabase)
            {
                if (!string.IsNullOrEmpty(dto.LegacyDatabaseConnectionString))
                {
                    application.LegacyDatabaseConnectionString = _encryptionService.Encrypt(dto.LegacyDatabaseConnectionString);
                }
                application.LegacyDatabaseType = Enum.Parse<LegacyDatabaseType>(dto.LegacyDatabaseType);
                application.LegacyUserTableName = dto.LegacyUserTableName;
                application.LegacyUserIdColumn = dto.LegacyUserIdColumn;
                application.LegacyEmailColumn = dto.LegacyEmailColumn;
                application.LegacyUsernameColumn = !string.IsNullOrEmpty(dto.LegacyUsernameColumn) ? dto.LegacyUsernameColumn : null;
                application.LegacyPasswordColumn = dto.LegacyPasswordColumn;
                application.LegacyPasswordHashAlgorithm = Enum.Parse<PasswordHashAlgorithm>(dto.LegacyPasswordHashAlgorithm);
            }

            await _unitOfWork.Applications.AddAsync(application);
            await _unitOfWork.SaveChangesAsync();

            var result = MapToDto(application);
            result.CreatedAt = application.CreatedAt;

            return Result<ApplicationDto>.Success(result, $"Client Secret (save this, it won't be shown again): {clientSecret}");
        }
        catch (Exception ex)
        {
            return Result<ApplicationDto>.Failure($"Failed to create application: {ex.Message}");
        }
    }

    public async Task<Result<ApplicationDto>> GetApplicationByIdAsync(Guid applicationId)
    {
        var app = await _unitOfWork.Applications.GetByIdAsync(applicationId);
        return app != null
            ? Result<ApplicationDto>.Success(MapToDto(app))
            : Result<ApplicationDto>.Failure("Application not found");
    }

    public async Task<Result<ApplicationDto>> GetApplicationByClientIdAsync(string clientId)
    {
        var apps = await _unitOfWork.Applications.FindAsync(a => a.ClientId == clientId && a.IsActive);
        var app = apps.FirstOrDefault();

        return app != null
            ? Result<ApplicationDto>.Success(MapToDto(app))
            : Result<ApplicationDto>.Failure("Application not found");
    }

    public async Task<Result<IEnumerable<ApplicationDto>>> GetApplicationsByTenantAsync(Guid tenantId)
    {
        var apps = await _unitOfWork.Applications.FindAsync(a => a.TenantId == tenantId);

        return Result<IEnumerable<ApplicationDto>>.Success(apps.ToList().Select(x => MapToDto(x)));
    }

    public async Task<Result<ApplicationDto>> UpdateApplicationAsync(Guid applicationId, UpdateApplicationDto dto)
    {
        try
        {
            var app = await _unitOfWork.Applications.GetByIdAsync(applicationId);
            if (app == null)
                return Result<ApplicationDto>.Failure("Application not found");

            // Basic Information
            if (!string.IsNullOrEmpty(dto.Name))
                app.Name = dto.Name;

            app.Description = dto.Description ?? app.Description;
            app.RedirectUris = dto.RedirectUris ?? app.RedirectUris;
            app.PostLogoutRedirectUris = dto.PostLogoutRedirectUris ?? app.PostLogoutRedirectUris;

            if (dto.AccessTokenLifetimeSeconds.HasValue)
                app.AccessTokenLifetimeSeconds = dto.AccessTokenLifetimeSeconds.Value;

            if (dto.RefreshTokenLifetimeSeconds.HasValue)
                app.RefreshTokenLifetimeSeconds = dto.RefreshTokenLifetimeSeconds.Value;

            // Google Configuration
            if (dto.GoogleEnabled.HasValue)
                app.GoogleEnabled = dto.GoogleEnabled.Value;

            if (app.GoogleEnabled)
            {
                if (!string.IsNullOrEmpty(dto.GoogleClientId))
                    app.GoogleClientId = dto.GoogleClientId;

                if (!string.IsNullOrEmpty(dto.GoogleClientSecret))
                    app.GoogleClientSecret = _encryptionService.Encrypt(dto.GoogleClientSecret);
            }
            else
            {
                app.GoogleClientId = null;
                app.GoogleClientSecret = null;
            }

            // Apple Configuration
            if (dto.AppleEnabled.HasValue)
                app.AppleEnabled = dto.AppleEnabled.Value;

            if (app.AppleEnabled)
            {
                if (!string.IsNullOrEmpty(dto.AppleClientId))
                    app.AppleClientId = dto.AppleClientId;

                if (!string.IsNullOrEmpty(dto.AppleTeamId))
                    app.AppleTeamId = dto.AppleTeamId;

                if (!string.IsNullOrEmpty(dto.AppleKeyId))
                    app.AppleKeyId = dto.AppleKeyId;

                if (!string.IsNullOrEmpty(dto.ApplePrivateKey))
                    app.ApplePrivateKey = _encryptionService.Encrypt(dto.ApplePrivateKey);
            }
            else
            {
                app.AppleClientId = null;
                app.AppleTeamId = null;
                app.AppleKeyId = null;
                app.ApplePrivateKey = null;
            }

            // LinkedIn Configuration
            if (dto.LinkedInEnabled.HasValue)
                app.LinkedInEnabled = dto.LinkedInEnabled.Value;

            if (app.LinkedInEnabled)
            {
                if (!string.IsNullOrEmpty(dto.LinkedInClientId))
                    app.LinkedInClientId = dto.LinkedInClientId;

                if (!string.IsNullOrEmpty(dto.LinkedInClientSecret))
                    app.LinkedInClientSecret = _encryptionService.Encrypt(dto.LinkedInClientSecret);
            }
            else
            {
                app.LinkedInClientId = null;
                app.LinkedInClientSecret = null;
            }

            // Legacy Database Configuration
            if (dto.HasLegacyDatabase.HasValue)
                app.HasLegacyDatabase = dto.HasLegacyDatabase.Value;

            if (app.HasLegacyDatabase)
            {
                if (!string.IsNullOrEmpty(dto.LegacyDatabaseConnectionString))
                    app.LegacyDatabaseConnectionString = _encryptionService.Encrypt(dto.LegacyDatabaseConnectionString);

                if (!string.IsNullOrEmpty(dto.LegacyDatabaseType))
                    app.LegacyDatabaseType = Enum.Parse<LegacyDatabaseType>(dto.LegacyDatabaseType);

                if (!string.IsNullOrEmpty(dto.LegacyUserTableName))
                    app.LegacyUserTableName = dto.LegacyUserTableName;

                if (!string.IsNullOrEmpty(dto.LegacyUserIdColumn))
                    app.LegacyUserIdColumn = dto.LegacyUserIdColumn;

                if (!string.IsNullOrEmpty(dto.LegacyEmailColumn))
                    app.LegacyEmailColumn = dto.LegacyEmailColumn;

                app.LegacyUsernameColumn = dto.LegacyUsernameColumn;

                if (!string.IsNullOrEmpty(dto.LegacyPasswordColumn))
                    app.LegacyPasswordColumn = dto.LegacyPasswordColumn;

                if (!string.IsNullOrEmpty(dto.LegacyPasswordHashAlgorithm))
                    app.LegacyPasswordHashAlgorithm = Enum.Parse<PasswordHashAlgorithm>(dto.LegacyPasswordHashAlgorithm);
            }
            else
            {
                app.LegacyDatabaseConnectionString = null;
                app.LegacyDatabaseType = null;
                app.LegacyUserTableName = null;
                app.LegacyUserIdColumn = null;
                app.LegacyEmailColumn = null;
                app.LegacyUsernameColumn = null;
                app.LegacyPasswordColumn = null;
                app.LegacyPasswordHashAlgorithm = null;
            }

            if (dto.IsActive.HasValue)
                app.IsActive = dto.IsActive.Value;

            _unitOfWork.Applications.Update(app);
            await _unitOfWork.SaveChangesAsync();

            return Result<ApplicationDto>.Success(MapToDto(app));
        }
        catch (Exception ex)
        {
            return Result<ApplicationDto>.Failure($"Failed to update application: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteApplicationAsync(Guid applicationId)
    {
        try
        {
            var app = await _unitOfWork.Applications.GetByIdAsync(applicationId);
            if (app == null)
                return Result<bool>.Failure("Application not found");

            _unitOfWork.Applications.Remove(app);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete application: {ex.Message}");
        }
    }

    public async Task<Result<string>> RegenerateClientSecretAsync(Guid applicationId)
    {
        try
        {
            var app = await _unitOfWork.Applications.GetByIdAsync(applicationId);
            if (app == null)
                return Result<string>.Failure("Application not found");

            var newSecret = GenerateClientSecret();
            app.ClientSecretHash = BCrypt.Net.BCrypt.HashPassword(newSecret);

            _unitOfWork.Applications.Update(app);
            await _unitOfWork.SaveChangesAsync();

            return Result<string>.Success(newSecret, "Save this secret, it won't be shown again");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Failed to regenerate secret: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ValidateClientCredentialsAsync(string clientId, string clientSecret)
    {
        var apps = await _unitOfWork.Applications.FindAsync(a => a.ClientId == clientId && a.IsActive);
        var app = apps.FirstOrDefault();

        if (app == null)
            return Result<bool>.Failure("Invalid client");

        var isValid = BCrypt.Net.BCrypt.Verify(clientSecret, app.ClientSecretHash);
        return isValid
            ? Result<bool>.Success(true)
            : Result<bool>.Failure("Invalid client credentials");
    }

    private string GenerateClientId()
    {
        return $"client_{Guid.NewGuid():N}";
    }

    private string GenerateClientSecret()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray()) +
               Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    private ApplicationDto MapToDto(Domain.Entities.Applications.Application app)
    {
        return new ApplicationDto
        {
            Id = app.Id,
            TenantId = app.TenantId,
            Name = app.Name,
            Description = app.Description,
            ClientId = app.ClientId,
            ApplicationType = app.ApplicationType.ToString(),
            AllowedGrantTypes = app.AllowedGrantTypes,
            AllowedScopes = app.AllowedScopes,
            RedirectUris = app.RedirectUris,
            PostLogoutRedirectUris = app.PostLogoutRedirectUris,
            AccessTokenLifetimeSeconds = app.AccessTokenLifetimeSeconds,
            RefreshTokenLifetimeSeconds = app.RefreshTokenLifetimeSeconds,

            // Google Configuration
            GoogleEnabled = app.GoogleEnabled,
            GoogleClientId = app.GoogleClientId,
            // GoogleClientSecret is never returned for security

            // Apple Configuration
            AppleEnabled = app.AppleEnabled,
            AppleClientId = app.AppleClientId,
            AppleTeamId = app.AppleTeamId,
            AppleKeyId = app.AppleKeyId,
            // ApplePrivateKey is never returned for security

            // LinkedIn Configuration
            LinkedInEnabled = app.LinkedInEnabled,
            LinkedInClientId = app.LinkedInClientId,
            // LinkedInClientSecret is never returned for security

            // Legacy Database Configuration
            HasLegacyDatabase = app.HasLegacyDatabase,
            LegacyDatabaseType = app.LegacyDatabaseType?.ToString(),
            LegacyUserTableName = app.LegacyUserTableName,
            LegacyUserIdColumn = app.LegacyUserIdColumn,
            LegacyEmailColumn = app.LegacyEmailColumn,
            LegacyUsernameColumn = app.LegacyUsernameColumn,
            LegacyPasswordColumn = app.LegacyPasswordColumn,
            LegacyPasswordHashAlgorithm = app.LegacyPasswordHashAlgorithm?.ToString(),
            // LegacyDatabaseConnectionString is never returned for security

            IsActive = app.IsActive,
            CreatedAt = app.CreatedAt
        };
    }
}