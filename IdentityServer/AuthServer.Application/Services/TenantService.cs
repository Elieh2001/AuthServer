using AuthServer.Application.DTOs.Common;
using AuthServer.Application.DTOs.Tenants;
using AuthServer.Application.Interfaces;
using AuthServer.Domain.Entities.Tenants;
using AuthServer.Domain.Interfaces;
using static AuthServer.Domain.Enumerations.Enums;

namespace AuthServer.Application.Services;

public class TenantService : ITenantService
{
    private readonly IUnitOfWork _unitOfWork;

    public TenantService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TenantDto>> CreateTenantAsync(CreateTenantDto dto)
    {
        try
        {
            var existingTenant = await _unitOfWork.Tenants
                .FirstOrDefaultAsync(t => t.Subdomain == dto.Subdomain);

            if (existingTenant != null)
                return Result<TenantDto>.Failure("Subdomain already exists");

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Subdomain = dto.Subdomain,
                SubscriptionPlan = dto.SubscriptionPlan,
                LogoUrl = dto.LogoUrl,
                PrimaryColor = dto.PrimaryColor,
                Status = TenantStatus.Active
            };

            await _unitOfWork.Tenants.AddAsync(tenant);
            await _unitOfWork.SaveChangesAsync();

            return Result<TenantDto>.Success(MapToDto(tenant));
        }
        catch (Exception ex)
        {
            return Result<TenantDto>.Failure($"Failed to create tenant: {ex.Message}");
        }
    }

    public async Task<Result<TenantDto>> GetTenantByIdAsync(Guid tenantId)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
        return tenant != null
            ? Result<TenantDto>.Success(MapToDto(tenant))
            : Result<TenantDto>.Failure("Tenant not found");
    }

    public async Task<Result<TenantDto>> GetTenantBySubdomainAsync(string subdomain)
    {
        var tenant = await _unitOfWork.Tenants
            .FirstOrDefaultAsync(t => t.Subdomain == subdomain && !t.IsDeleted);

        return tenant != null
            ? Result<TenantDto>.Success(MapToDto(tenant))
            : Result<TenantDto>.Failure("Tenant not found");
    }

    public async Task<Result<IEnumerable<TenantDto>>> GetAllTenantsAsync()
    {
        var tenants = await _unitOfWork.Tenants.GetAllAsync();
        return Result<IEnumerable<TenantDto>>.Success(tenants.Select(MapToDto));
    }

    public async Task<Result<TenantDto>> UpdateTenantAsync(Guid tenantId, UpdateTenantDto dto)
    {
        try
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null)
                return Result<TenantDto>.Failure("Tenant not found");

            if (!string.IsNullOrEmpty(dto.Name))
                tenant.Name = dto.Name;

            if (!string.IsNullOrEmpty(dto.SubscriptionPlan))
                tenant.SubscriptionPlan = dto.SubscriptionPlan;

            if (dto.PasswordMinLength.HasValue)
                tenant.PasswordMinLength = dto.PasswordMinLength.Value;

            if (dto.SessionTimeoutMinutes.HasValue)
                tenant.SessionTimeoutMinutes = dto.SessionTimeoutMinutes.Value;

            tenant.LogoUrl = dto.LogoUrl ?? tenant.LogoUrl;
            tenant.PrimaryColor = dto.PrimaryColor ?? tenant.PrimaryColor;

            _unitOfWork.Tenants.Update(tenant);
            await _unitOfWork.SaveChangesAsync();

            return Result<TenantDto>.Success(MapToDto(tenant));
        }
        catch (Exception ex)
        {
            return Result<TenantDto>.Failure($"Failed to update tenant: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteTenantAsync(Guid tenantId)
    {
        try
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null)
                return Result<bool>.Failure("Tenant not found");

            tenant.IsDeleted = true;
            tenant.DeletedAt = DateTime.UtcNow;
            _unitOfWork.Tenants.Update(tenant);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to delete tenant: {ex.Message}");
        }
    }

    public async Task<Result<bool>> SuspendTenantAsync(Guid tenantId)
    {
        return await UpdateTenantStatusAsync(tenantId, TenantStatus.Suspended);
    }

    public async Task<Result<bool>> ActivateTenantAsync(Guid tenantId)
    {
        return await UpdateTenantStatusAsync(tenantId, TenantStatus.Active);
    }

    private async Task<Result<bool>> UpdateTenantStatusAsync(Guid tenantId, TenantStatus status)
    {
        try
        {
            var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
            if (tenant == null)
                return Result<bool>.Failure("Tenant not found");

            tenant.Status = status;
            _unitOfWork.Tenants.Update(tenant);
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to update tenant status: {ex.Message}");
        }
    }

    private TenantDto MapToDto(Tenant tenant)
    {
        return new TenantDto
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain,
            Status = tenant.Status.ToString(),
            SubscriptionPlan = tenant.SubscriptionPlan,
            PasswordMinLength = tenant.PasswordMinLength,
            PasswordRequireUppercase = tenant.PasswordRequireUppercase,
            PasswordRequireLowercase = tenant.PasswordRequireLowercase,
            PasswordRequireDigit = tenant.PasswordRequireDigit,
            PasswordRequireSpecialChar = tenant.PasswordRequireSpecialChar,
            SessionTimeoutMinutes = tenant.SessionTimeoutMinutes,
            MaxFailedLoginAttempts = tenant.MaxFailedLoginAttempts,
            AccountLockoutDurationMinutes = tenant.AccountLockoutDurationMinutes,
            LogoUrl = tenant.LogoUrl,
            PrimaryColor = tenant.PrimaryColor,
            CreatedAt = tenant.CreatedAt,
            UpdatedAt = tenant.UpdatedAt
        };
    }
}

