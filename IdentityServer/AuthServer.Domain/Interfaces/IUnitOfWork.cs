using AuthServer.Domain.Entities.Applications;
using AuthServer.Domain.Entities.System;
using AuthServer.Domain.Entities.Tenants;
using AuthServer.Domain.Entities.Tokens;
using AuthServer.Domain.Entities.Users;

namespace AuthServer.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repositories
    IRepository<Tenant> Tenants { get; }
    IRepository<Application> Applications { get; }
    IRepository<User> Users { get; }
    IRepository<ExternalLogin> ExternalLogins { get; }
    IRepository<ApplicationUserMapping> ApplicationUserMappings { get; }
    IRepository<RefreshToken> RefreshTokens { get; }
    IRepository<TenantAdmin> TenantAdmins { get; }
    IRepository<VerificationToken> VerificationTokens { get; }
    IRepository<AuditLog> AuditLogs { get; }

    // Save changes
    int SaveChanges();
    Task<int> SaveChangesAsync();

    // Transaction support
    void BeginTransaction();
    void Commit();
    void Rollback();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
