using AuthServer.Domain.Entities.Applications;
using AuthServer.Domain.Entities.System;
using AuthServer.Domain.Entities.Tenants;
using AuthServer.Domain.Entities.Tokens;
using AuthServer.Domain.Entities.Users;
using AuthServer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthServer.Infrastructure.Data.Repositories;

    public class UnitOfWork : IUnitOfWork
{
    private readonly AuthServerDbContext _context;
    private IDbContextTransaction _transaction;

    // Repository instances
    private IRepository<Tenant> _tenants;
    private IRepository<Application> _applications;
    private IRepository<User> _users;
    private IRepository<ExternalLogin> _externalLogins;
    private IRepository<ApplicationUserMapping> _applicationUserMappings;
    private IRepository<RefreshToken> _refreshTokens;
    private IRepository<TenantAdmin> _tenantAdmins;
    private IRepository<VerificationToken> _verificationTokens;
    private IRepository<AuditLog> _auditLogs;

    public UnitOfWork(AuthServerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    // Lazy initialization of repositories
    public IRepository<Tenant> Tenants =>
        _tenants ??= new Repository<Tenant>(_context);

    public IRepository<Application> Applications =>
        _applications ??= new Repository<Application>(_context);

    public IRepository<User> Users =>
        _users ??= new Repository<User>(_context);

    public IRepository<ExternalLogin> ExternalLogins =>
        _externalLogins ??= new Repository<ExternalLogin>(_context);

    public IRepository<ApplicationUserMapping> ApplicationUserMappings =>
        _applicationUserMappings ??= new Repository<ApplicationUserMapping>(_context);

    public IRepository<RefreshToken> RefreshTokens =>
        _refreshTokens ??= new Repository<RefreshToken>(_context);

    public IRepository<TenantAdmin> TenantAdmins =>
        _tenantAdmins ??= new Repository<TenantAdmin>(_context);

    public IRepository<VerificationToken> VerificationTokens =>
        _verificationTokens ??= new Repository<VerificationToken>(_context);

    public IRepository<AuditLog> AuditLogs =>
        _auditLogs ??= new Repository<AuditLog>(_context);

    // Save changes
    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    // Transaction support
    public void BeginTransaction()
    {
        _transaction = _context.Database.BeginTransaction();
    }

    public void Commit()
    {
        try
        {
            _context.SaveChanges();
            _transaction?.Commit();
        }
        catch
        {
            Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public void Rollback()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    // Dispose
    public void Dispose()
    {
        _transaction?.Dispose();
        _context?.Dispose();
    }
}
