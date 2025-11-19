using AuthServer.Application.DTOs.Common;
using AuthServer.Application.DTOs.Tenants;

namespace AuthServer.Application.Interfaces;

public interface ITenantService
{
    Task<Result<TenantDto>> CreateTenantAsync(CreateTenantDto dto);
    Task<Result<TenantDto>> GetTenantByIdAsync(Guid tenantId);
    Task<Result<TenantDto>> GetTenantBySubdomainAsync(string subdomain);
    Task<Result<IEnumerable<TenantDto>>> GetAllTenantsAsync();
    Task<Result<TenantDto>> UpdateTenantAsync(Guid tenantId, UpdateTenantDto dto);
    Task<Result<bool>> DeleteTenantAsync(Guid tenantId);
    Task<Result<bool>> SuspendTenantAsync(Guid tenantId);
    Task<Result<bool>> ActivateTenantAsync(Guid tenantId);

}
