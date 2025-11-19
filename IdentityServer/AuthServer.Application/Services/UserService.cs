using AuthServer.Application.DTOs.Common;
using AuthServer.Application.DTOs.Users;
using AuthServer.Application.Interfaces;
using AuthServer.Domain.Entities.Users;
using AuthServer.Domain.Interfaces;
using static AuthServer.Domain.Enumerations.Enums;

namespace AuthServer.Application.Services;

public class UserService : IUserService
{
    #region Members

    private readonly IUnitOfWork _unitOfWork;

    #endregion

    #region Constructors

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    #endregion

    #region Public Methods

    public async Task<Result<UserDto>> CreateUserAsync(CreateUserDto dto)
    {
        try
        {
            var existing = await _unitOfWork.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.TenantId == dto.TenantId);

            if (existing != null)
                return Result<UserDto>.Failure("User already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                TenantId = dto.TenantId,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                EmailVerified = dto.EmailVerified
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return Result<UserDto>.Success(MapToDto(user));
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to create user: {ex.Message}");
        }
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        return user != null
            ? Result<UserDto>.Success(MapToDto(user))
            : Result<UserDto>.Failure("User not found");
    }

    public async Task<Result<UserDto>> GetUserByEmailAsync(string email, Guid tenantId)
    {
        var user = await _unitOfWork.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.TenantId == tenantId);

        return user != null
            ? Result<UserDto>.Success(MapToDto(user))
            : Result<UserDto>.Failure("User not found");
    }

    public async Task<Result<IEnumerable<UserDto>>> GetUsersByTenantAsync(Guid tenantId, int page = 1, int pageSize = 20)
    {
        var users =  _unitOfWork.Users
            .Query()
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);            

        return Result<IEnumerable<UserDto>>.Success(users.Select(MapToDto));
    }

    public async Task<Result<UserDto>> UpdateUserAsync(Guid userId, UpdateUserDto dto)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return Result<UserDto>.Failure("User not found");

            if (!string.IsNullOrEmpty(dto.FirstName))
                user.FirstName = dto.FirstName;

            if (!string.IsNullOrEmpty(dto.LastName))
                user.LastName = dto.LastName;

            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            if (dto.EmailVerified.HasValue)
                user.EmailVerified = dto.EmailVerified.Value;

            if (dto.IsActive.HasValue)
                user.IsActive = dto.IsActive.Value;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Result<UserDto>.Success(MapToDto(user));
        }
        catch (Exception ex)
        {
            return Result<UserDto>.Failure($"Failed to update user: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteUserAsync(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found");

            _unitOfWork.Users.Remove(user);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete user: {ex.Message}");
        }
    }

    public async Task<Result<bool>> LockUserAsync(Guid userId, int durationMinutes)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found");

            user.LockoutEnd = DateTime.UtcNow.AddMinutes(durationMinutes);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to lock user: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UnlockUserAsync(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return Result<bool>.Failure("User not found");

            user.LockoutEnd = null;
            user.AccessFailedCount = 0;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to unlock user: {ex.Message}");
        }
    }

    public async Task<Result<bool>> LinkExternalProviderAsync(Guid userId, LinkExternalProviderDto dto)
    {
        try
        {
            var existing = await _unitOfWork.ExternalLogins
                .FirstOrDefaultAsync(el => el.Provider.ToString() == dto.Provider &&
                                           el.ProviderUserId == dto.ProviderUserId);

            if (existing != null)
                return Result<bool>.Failure("This external account is already linked");

            var externalLogin = new ExternalLogin
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Provider = Enum.Parse<ExternalProvider>(dto.Provider),
                ProviderUserId = dto.ProviderUserId,
                ProviderEmail = dto.ProviderEmail,
                ProviderDisplayName = dto.ProviderDisplayName
            };

            await _unitOfWork.ExternalLogins.AddAsync(externalLogin);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to link provider: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UnlinkExternalProviderAsync(Guid userId, string provider)
    {
        try
        {
            var externalLogin = await _unitOfWork.ExternalLogins
                .FirstOrDefaultAsync(el => el.UserId == userId && el.Provider.ToString() == provider);

            if (externalLogin == null)
                return Result<bool>.Failure("External login not found");

            _unitOfWork.ExternalLogins.Remove(externalLogin);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to unlink provider: {ex.Message}");
        }
    }

    #endregion

    #region Private Methods 

    private UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            TenantId = user.TenantId,
            Email = user.Email,
            EmailVerified = user.EmailVerified,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            IsLockedOut = user.IsLockedOut(),
            LockoutEnd = user.LockoutEnd,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };
    }

    #endregion
}