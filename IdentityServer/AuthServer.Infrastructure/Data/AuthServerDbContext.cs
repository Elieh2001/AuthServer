using AuthServer.Domain.Entities;
using AuthServer.Domain.Entities.Applications;
using AuthServer.Domain.Entities.System;
using AuthServer.Domain.Entities.Tenants;
using AuthServer.Domain.Entities.Tokens;
using AuthServer.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Infrastructure.Data;

public class AuthServerDbContext : DbContext
{
    private readonly Guid? _currentTenantId;

    public AuthServerDbContext(DbContextOptions<AuthServerDbContext> options)
        : base(options)
    {
    }

    public AuthServerDbContext(DbContextOptions<AuthServerDbContext> options, Guid? currentTenantId)
        : base(options)
    {
        _currentTenantId = currentTenantId;
    }

    // DbSets
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<TenantApplication> TenantApplications { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ExternalLogin> ExternalLogins { get; set; }
    public DbSet<ApplicationUserMapping> ApplicationUserMappings { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<TenantAdmin> TenantAdmins { get; set; }
    public DbSet<VerificationToken> VerificationTokens { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from the assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthServerDbContext).Assembly);

        // Global query filters for multi-tenancy
        if (_currentTenantId.HasValue)
        {
            // Apply tenant filter to TenantEntity derived entities
            // Note: Users can have null TenantId (system admins)
            modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == _currentTenantId.Value || u.TenantId == null);
            modelBuilder.Entity<RefreshToken>().HasQueryFilter(rt => rt.TenantId == _currentTenantId.Value || rt.TenantId == null);
        }

        // Global query filter for soft delete
        modelBuilder.Entity<Tenant>().HasQueryFilter(t => !t.IsDeleted);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}