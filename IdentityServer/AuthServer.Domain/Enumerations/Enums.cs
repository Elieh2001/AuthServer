namespace AuthServer.Domain.Enumerations;

public class Enums
{
    public enum TenantStatus
    {
        Active,
        Suspended,
        Trial,
        Cancelled
    }

    public enum ApplicationType
    {
        Native,           // Uses auth server's user database
        LegacyDatabase,   // Has own user table
        Federated         // External providers only
    }

    public enum ExternalProvider
    {
        Google,
        Apple,
        LinkedIn
    }

    public enum PasswordHashAlgorithm
    {
        MD5,
        SHA1,
        SHA256,
        SHA512,
        BCrypt,
        PBKDF2,
        AspNetIdentity  // ASP.NET Core Identity default
    }

    public enum AdminRole
    {
        SuperAdmin,      // Access to all tenants
        TenantAdmin,     // Access to specific tenant
        ApplicationAdmin // Access to specific applications
    }

    public enum TokenType
    {
        EmailVerification,
        PasswordReset,
        TwoFactorAuthentication
    }

    public enum AuditEventType
    {
        Login,
        LoginFailed,
        Logout,
        TokenIssued,
        TokenRefreshed,
        TokenRevoked,
        PasswordChanged,
        PasswordResetRequested,
        EmailVerified,
        ExternalLoginAdded,
        ExternalLoginRemoved,
        UserCreated,
        UserUpdated,
        UserDeleted,
        UserLocked,
        UserUnlocked,
        TwoFactorEnabled,
        TwoFactorDisabled,
        ApplicationCreated,
        ApplicationUpdated,
        ApplicationDeleted,
        TenantCreated,
        TenantUpdated,
        TenantSuspended
    }

    public enum AuditEventCategory
    {
        Authentication,
        Authorization,
        Administration,
        Security,
        UserManagement,
        ApplicationManagement,
        TenantManagement
    }

    public enum LegacyDatabaseType
    {
        SqlServer,
        PostgreSQL,
        MySQL,
        Oracle
    }
}
