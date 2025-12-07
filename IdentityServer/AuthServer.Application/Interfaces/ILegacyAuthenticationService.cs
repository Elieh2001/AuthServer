using AuthServer.Application.DTOs.Common;
using AuthServer.Application.DTOs.Users;
using AuthServer.Application.Services;

namespace AuthServer.Application.Interfaces;

public interface ILegacyAuthenticationService
{
    Task<Result<LegacyUserDto>> AuthenticateAgainstLegacyDbAsync(Guid applicationId, string emailOrUsername, string password);
    Task<bool> TestLegacyConnectionAsync(Guid applicationId);
}
