using AuthServer.Application.DTOs.Applications;
using AuthServer.Application.DTOs.Common;

namespace AuthServer.Application.Interfaces;

public interface IApplicationService
{
    Task<Result<ApplicationDto>> CreateApplicationAsync(CreateApplicationDto dto);
    Task<Result<ApplicationDto>> GetApplicationByIdAsync(Guid applicationId);
    Task<Result<ApplicationDto>> GetApplicationByClientIdAsync(string clientId);
    Task<Result<IEnumerable<ApplicationDto>>> GetApplicationsByTenantAsync(Guid tenantId);
    Task<Result<ApplicationDto>> UpdateApplicationAsync(Guid applicationId, UpdateApplicationDto dto);
    Task<Result<bool>> DeleteApplicationAsync(Guid applicationId);
    Task<Result<string>> RegenerateClientSecretAsync(Guid applicationId);
    Task<Result<bool>> ValidateClientCredentialsAsync(string clientId, string clientSecret);

}
