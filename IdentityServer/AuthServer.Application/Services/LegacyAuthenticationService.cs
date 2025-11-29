using AuthServer.Application.DTOs.Common;
using AuthServer.Application.DTOs.Users;
using AuthServer.Application.Interfaces;
using AuthServer.Domain.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static AuthServer.Domain.Enumerations.Enums;

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

            if (!application.LegacyDatabaseType.HasValue || !application.LegacyPasswordHashAlgorithm.HasValue)
                return Result<LegacyUserDto>.Failure("Legacy database configuration incomplete");

            var connectionString = _encryptionService.Decrypt(application.LegacyDatabaseConnectionString);

            // Create connection based on database type
            using var connection = CreateConnection(application.LegacyDatabaseType.Value, connectionString);
            if (connection is DbConnection dbConnection)
            {
                await dbConnection.OpenAsync();
            }
            else
            {
                connection.Open();
            }

            // Build query to fetch user
            var query = $@"
                SELECT
                    {application.LegacyUserIdColumn} as UserId,
                    {application.LegacyEmailColumn} as Email,
                    {application.LegacyPasswordColumn} as PasswordHash
                FROM {application.LegacyUserTableName}
                WHERE {application.LegacyEmailColumn} = @Email";

            // Add additional columns if configured
            var additionalColumns = new List<string>();
            if (!string.IsNullOrEmpty(application.LegacyAdditionalColumnsMapping))
            {
                try
                {
                    var columnsMapping = JsonSerializer.Deserialize<Dictionary<string, string>>(
                        application.LegacyAdditionalColumnsMapping);

                    if (columnsMapping != null)
                    {
                        foreach (var mapping in columnsMapping)
                        {
                            query = query.Replace("PasswordHash", $"PasswordHash, {mapping.Key} as {mapping.Value}");
                            additionalColumns.Add(mapping.Value);
                        }
                    }
                }
                catch
                {
                    // Ignore JSON parsing errors
                }
            }

            // Execute query
            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(query, new { Email = email });

            if (result == null)
                return Result<LegacyUserDto>.Failure("User not found in legacy database");

            // Get the password hash from result
            var storedPasswordHash = ((IDictionary<string, object>)result)["PasswordHash"]?.ToString();

            if (string.IsNullOrEmpty(storedPasswordHash))
                return Result<LegacyUserDto>.Failure("Password hash not found");

            // Verify password
            bool isPasswordValid = VerifyPassword(
                password,
                storedPasswordHash,
                application.LegacyPasswordHashAlgorithm.Value);

            if (!isPasswordValid)
                return Result<LegacyUserDto>.Failure("Invalid credentials");

            // Build LegacyUserDto
            var legacyUser = new LegacyUserDto
            {
                LegacyUserId = ((IDictionary<string, object>)result)["UserId"]?.ToString() ?? string.Empty,
                Email = ((IDictionary<string, object>)result)["Email"]?.ToString() ?? string.Empty
            };

            // Extract first name and last name from additional columns if available
            var resultDict = (IDictionary<string, object>)result;

            if (resultDict.ContainsKey("FirstName"))
                legacyUser.FirstName = resultDict["FirstName"]?.ToString() ?? string.Empty;

            if (resultDict.ContainsKey("LastName"))
                legacyUser.LastName = resultDict["LastName"]?.ToString() ?? string.Empty;

            // Add all additional data
            foreach (var column in additionalColumns)
            {
                if (resultDict.ContainsKey(column))
                {
                    legacyUser.AdditionalData[column] = resultDict[column] ?? string.Empty;
                }
            }

            return Result<LegacyUserDto>.Success(legacyUser);
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
            if (application == null || !application.HasLegacyDatabase || !application.LegacyDatabaseType.HasValue)
                return false;

            var connectionString = _encryptionService.Decrypt(application.LegacyDatabaseConnectionString);

            using var connection = CreateConnection(application.LegacyDatabaseType.Value, connectionString);
            if (connection is DbConnection dbConnection)
            {
                await dbConnection.OpenAsync();
            }
            else
            {
                connection.Open();
            }

            // Test with a simple query
            var result = await connection.QueryFirstOrDefaultAsync<int>("SELECT 1");

            return result == 1;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Private Methods

    private IDbConnection CreateConnection(LegacyDatabaseType dbType, string connectionString)
    {
        return dbType switch
        {
            LegacyDatabaseType.SqlServer => new SqlConnection(connectionString),
            LegacyDatabaseType.PostgreSQL => new NpgsqlConnection(connectionString),
            LegacyDatabaseType.MySQL => new MySqlConnection(connectionString),
            LegacyDatabaseType.Oracle => new OracleConnection(connectionString),
            _ => throw new NotSupportedException($"Database type {dbType} is not supported")
        };
    }

    private bool VerifyPassword(string password, string storedHash, PasswordHashAlgorithm algorithm)
    {
        return algorithm switch
        {
            PasswordHashAlgorithm.MD5 => VerifyHashWithAlgorithm(password, storedHash, MD5.Create()),
            PasswordHashAlgorithm.SHA1 => VerifyHashWithAlgorithm(password, storedHash, SHA1.Create()),
            PasswordHashAlgorithm.SHA256 => VerifyHashWithAlgorithm(password, storedHash, SHA256.Create()),
            PasswordHashAlgorithm.SHA512 => VerifyHashWithAlgorithm(password, storedHash, SHA512.Create()),
            PasswordHashAlgorithm.BCrypt => VerifyBCrypt(password, storedHash),
            PasswordHashAlgorithm.PBKDF2 => VerifyPBKDF2(password, storedHash),
            PasswordHashAlgorithm.AspNetIdentity => VerifyAspNetIdentity(password, storedHash),
            _ => throw new NotSupportedException($"Hash algorithm {algorithm} is not supported")
        };
    }

    private bool VerifyHashWithAlgorithm(string password, string storedHash, HashAlgorithm algorithm)
    {
        try
        {
            using (algorithm)
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var hashBytes = algorithm.ComputeHash(passwordBytes);
                var computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

                return string.Equals(computedHash, storedHash.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            return false;
        }
    }

    private bool VerifyBCrypt(string password, string storedHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
        catch
        {
            return false;
        }
    }

    private bool VerifyPBKDF2(string password, string storedHash)
    {
        try
        {
            // PBKDF2 format: iterations:salt:hash
            var parts = storedHash.Split(':');
            if (parts.Length != 3)
                return false;

            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] storedHashBytes = Convert.FromBase64String(parts[2]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] computedHash = pbkdf2.GetBytes(storedHashBytes.Length);

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHashBytes);
        }
        catch
        {
            return false;
        }
    }

    private bool VerifyAspNetIdentity(string password, string storedHash)
    {
        try
        {
            // ASP.NET Identity uses a specific format for password hashes
            // Format: 0x01 (version) + salt (16 bytes) + subkey (32 bytes)
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            if (hashBytes.Length != 49) // 1 + 16 + 32
                return false;

            byte version = hashBytes[0];
            if (version != 0x01)
                return false;

            byte[] salt = new byte[16];
            Buffer.BlockCopy(hashBytes, 1, salt, 0, 16);

            byte[] storedSubkey = new byte[32];
            Buffer.BlockCopy(hashBytes, 17, storedSubkey, 0, 32);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] computedSubkey = pbkdf2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(computedSubkey, storedSubkey);
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
