using AuthServer.Application.DTOs.Common;
using AuthServer.Application.DTOs.Users;

namespace AuthServer.Application.Interfaces;

public interface IUserService
{
    Task<Result<UserDto>> CreateUserAsync(CreateUserDto dto);
    Task<Result<UserDto>> GetUserByIdAsync(Guid userId);
    Task<Result<UserDto>> GetUserByEmailAsync(string email, Guid tenantId);
    Task<Result<IEnumerable<UserDto>>> GetUsersByTenantAsync(Guid tenantId, int page = 1, int pageSize = 20);
    Task<Result<UserDto>> UpdateUserAsync(Guid userId, UpdateUserDto dto);
    Task<Result<bool>> DeleteUserAsync(Guid userId);
    Task<Result<bool>> LockUserAsync(Guid userId, int durationMinutes);
    Task<Result<bool>> UnlockUserAsync(Guid userId);
    Task<Result<bool>> LinkExternalProviderAsync(Guid userId, LinkExternalProviderDto dto);
    Task<Result<bool>> UnlinkExternalProviderAsync(Guid userId, string provider);

}
