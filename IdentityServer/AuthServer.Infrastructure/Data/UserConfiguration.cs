using AuthServer.Domain.Entities.Applications;
using AuthServer.Domain.Entities.Tenants;
using AuthServer.Domain.Entities.Tokens;
using AuthServer.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthServer.Infrastructure.Data;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500);

        builder.Property(u => u.Roles)
            .HasMaxLength(500);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(50);

        builder.Property(u => u.FirstName)
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .HasMaxLength(100);

        builder.Property(u => u.LastLoginIp)
            .HasMaxLength(45);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(u => u.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Unique index for tenant users (TenantId + Email must be unique)
        // System admins (null TenantId) can share emails across different contexts
        builder.HasIndex(u => new { u.TenantId, u.Email })
            .IsUnique()
            .HasDatabaseName("UQ_Users_Email_Tenant")
            .HasFilter("[TenantId] IS NOT NULL");

        builder.HasIndex(u => u.Email)
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => u.IsSystemAdmin)
            .HasDatabaseName("IX_Users_IsSystemAdmin");

        // Relationships - FIXED cascade delete issues
        builder.HasMany(u => u.ExternalLogins)
            .WithOne(el => el.User)
            .HasForeignKey(el => el.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ApplicationUserMappings)
            .WithOne(aum => aum.User)
            .HasForeignKey(aum => aum.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.VerificationTokens)
            .WithOne(vt => vt.User)
            .HasForeignKey(vt => vt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
public class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.ToTable("ExternalLogins");

        builder.HasKey(el => el.Id);

        builder.Property(el => el.Provider)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(el => el.ProviderUserId)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(el => el.ProviderEmail)
            .HasMaxLength(256);

        builder.Property(el => el.ProviderDisplayName)
            .HasMaxLength(200);

        builder.Property(el => el.LinkedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(el => el.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(el => el.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(el => new { el.Provider, el.ProviderUserId })
            .IsUnique()
            .HasDatabaseName("UQ_ExternalLogins_Provider_UserId");

        builder.HasIndex(el => el.UserId)
            .HasDatabaseName("IX_ExternalLogins_UserId");
    }
}
public class ApplicationUserMappingConfiguration : IEntityTypeConfiguration<ApplicationUserMapping>
{
    public void Configure(EntityTypeBuilder<ApplicationUserMapping> builder)
    {
        builder.ToTable("ApplicationUserMappings");

        builder.HasKey(aum => aum.Id);

        builder.Property(aum => aum.LegacyUserId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(aum => aum.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.Property(aum => aum.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(aum => aum.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(aum => new { aum.ApplicationId, aum.LegacyUserId })
            .IsUnique()
            .HasDatabaseName("UQ_AppUserMappings_App_LegacyUser");

        builder.HasIndex(aum => aum.UserId)
            .HasDatabaseName("IX_AppUserMappings_UserId");

        builder.HasIndex(aum => aum.ApplicationId)
            .HasDatabaseName("IX_AppUserMappings_ApplicationId");

        // Relationships - RESTRICT to prevent cascade cycles
        builder.HasOne(aum => aum.User)
            .WithMany(u => u.ApplicationUserMappings)
            .HasForeignKey(aum => aum.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(aum => aum.Application)
            .WithMany()
            .HasForeignKey(aum => aum.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.RevokedReason)
            .HasMaxLength(500);

        builder.Property(rt => rt.RevokedByIp)
            .HasMaxLength(45);

        builder.Property(rt => rt.CreatedByIp)
            .HasMaxLength(45);

        builder.Property(rt => rt.UserAgent)
            .HasMaxLength(500);

        builder.Property(rt => rt.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(rt => rt.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(rt => rt.TokenHash)
            .HasDatabaseName("IX_RefreshTokens_TokenHash");

        builder.HasIndex(rt => rt.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        builder.HasIndex(rt => rt.ExpiresAt)
            .HasDatabaseName("IX_RefreshTokens_ExpiresAt")
            .HasFilter("[IsRevoked] = 0");

        builder.HasIndex(rt => new { rt.ApplicationId, rt.UserId })
            .HasDatabaseName("IX_RefreshTokens_Application")
            .HasFilter("[IsRevoked] = 0 AND [ApplicationId] IS NOT NULL");

        // FIXED: Relationships with Restrict to prevent cascade cycles
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rt => rt.Application)
            .WithMany()
            .HasForeignKey(rt => rt.ApplicationId)
            .IsRequired(false) // Make ApplicationId optional
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(rt => rt.Tenant)
            .WithMany()
            .HasForeignKey(rt => rt.TenantId)
            .IsRequired(false) // Make TenantId optional for system admin tokens
            .OnDelete(DeleteBehavior.Restrict);

        // Self-referencing relationship for rotation
        builder.HasOne(rt => rt.ParentToken)
            .WithMany()
            .HasForeignKey(rt => rt.ParentTokenId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Subdomain)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.SubscriptionPlan)
            .HasMaxLength(100);

        builder.Property(t => t.LogoUrl)
            .HasMaxLength(500);

        builder.Property(t => t.PrimaryColor)
            .HasMaxLength(20);

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(t => t.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(t => t.Subdomain)
            .IsUnique()
            .HasDatabaseName("UQ_Tenants_Subdomain");

        builder.HasIndex(t => t.Name)
            .HasDatabaseName("IX_Tenants_Name");

        // Relationships
        builder.HasMany(t => t.TenantApplications)
            .WithOne(ta => ta.Tenant)
            .HasForeignKey(ta => ta.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.TenantAdmins)
            .WithOne(ta => ta.Tenant)
            .HasForeignKey(ta => ta.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
{
    public void Configure(EntityTypeBuilder<Application> builder)
    {
        builder.ToTable("Applications");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Description)
            .HasMaxLength(1000);

        builder.Property(a => a.ClientId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.ClientSecretHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.ApplicationType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(a => a.AllowedGrantTypes)
            .HasMaxLength(500);

        builder.Property(a => a.AllowedScopes)
            .HasMaxLength(500);

        builder.Property(a => a.RedirectUris)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.PostLogoutRedirectUris)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.AllowedCorsOrigins)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(a => a.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(a => a.ClientId)
            .IsUnique()
            .HasDatabaseName("UQ_Applications_ClientId");

        // Relationships
        builder.HasMany(a => a.TenantApplications)
            .WithOne(ta => ta.Application)
            .HasForeignKey(ta => ta.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.ApplicationUserMappings)
            .WithOne(aum => aum.Application)
            .HasForeignKey(aum => aum.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.RefreshTokens)
            .WithOne(rt => rt.Application)
            .HasForeignKey(rt => rt.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TenantApplicationConfiguration : IEntityTypeConfiguration<TenantApplication>
{
    public void Configure(EntityTypeBuilder<TenantApplication> builder)
    {
        builder.ToTable("TenantApplications");

        builder.HasKey(ta => ta.Id);

        builder.Property(ta => ta.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(ta => ta.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Unique constraint to prevent duplicate tenant-application mappings
        builder.HasIndex(ta => new { ta.TenantId, ta.ApplicationId })
            .IsUnique()
            .HasDatabaseName("UQ_TenantApplications_Tenant_Application");

        builder.HasIndex(ta => ta.TenantId)
            .HasDatabaseName("IX_TenantApplications_TenantId");

        builder.HasIndex(ta => ta.ApplicationId)
            .HasDatabaseName("IX_TenantApplications_ApplicationId");

        // Relationships configured in Tenant and Application configurations
    }
}