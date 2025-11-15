-- =============================================
-- TENANT MANAGEMENT
-- =============================================

CREATE TABLE Tenants (
    TenantId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Subdomain NVARCHAR(100) NOT NULL UNIQUE, -- e.g., 'companyA' for companyA.yourauth.com
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active', -- Active, Suspended, Trial, Cancelled
    SubscriptionPlan NVARCHAR(100) NULL, -- Basic, Premium, Enterprise
    
    -- Settings (could be JSON or separate table)
    PasswordMinLength INT NOT NULL DEFAULT 8,
    PasswordRequireUppercase BIT NOT NULL DEFAULT 1,
    PasswordRequireLowercase BIT NOT NULL DEFAULT 1,
    PasswordRequireDigit BIT NOT NULL DEFAULT 1,
    PasswordRequireSpecialChar BIT NOT NULL DEFAULT 1,
    
    SessionTimeoutMinutes INT NOT NULL DEFAULT 60,
    MaxFailedLoginAttempts INT NOT NULL DEFAULT 5,
    AccountLockoutDurationMinutes INT NOT NULL DEFAULT 30,
    
    -- Branding
    LogoUrl NVARCHAR(500) NULL,
    PrimaryColor NVARCHAR(7) NULL, -- Hex color
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2 NULL,
    
    INDEX IX_Tenants_Subdomain (Subdomain),
    INDEX IX_Tenants_Status (Status) WHERE IsDeleted = 0
);

-- =============================================
-- APPLICATION REGISTRATION
-- =============================================

CREATE TABLE Applications (
    ApplicationId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    
    -- OAuth Client Credentials
    ClientId NVARCHAR(100) NOT NULL UNIQUE,
    ClientSecretHash NVARCHAR(500) NOT NULL, -- Hashed
    
    -- Application Type
    ApplicationType NVARCHAR(50) NOT NULL, -- Native, LegacyDatabase, Federated
    
    -- OAuth Configuration
    AllowedGrantTypes NVARCHAR(500) NOT NULL, -- Comma-separated: authorization_code,refresh_token,client_credentials
    AllowedScopes NVARCHAR(500) NOT NULL, -- Comma-separated: openid,profile,email,offline_access
    RedirectUris NVARCHAR(MAX) NULL, -- JSON array of allowed callback URLs
    PostLogoutRedirectUris NVARCHAR(MAX) NULL,
    AllowedCorsOrigins NVARCHAR(MAX) NULL,
    
    -- Token Configuration (can override global settings)
    AccessTokenLifetimeSeconds INT NULL, -- NULL = use global default from appsettings
    RefreshTokenLifetimeSeconds INT NULL,
    RefreshTokenRotationEnabled BIT NOT NULL DEFAULT 1,
    RefreshTokenAbsoluteLifetimeSeconds INT NULL, -- Max lifetime regardless of rotation
    
    -- External Providers (per-application)
    GoogleEnabled BIT NOT NULL DEFAULT 0,
    GoogleClientId NVARCHAR(500) NULL,
    GoogleClientSecret NVARCHAR(500) NULL, -- Encrypted
    
    AppleEnabled BIT NOT NULL DEFAULT 0,
    AppleClientId NVARCHAR(500) NULL,
    AppleTeamId NVARCHAR(100) NULL,
    AppleKeyId NVARCHAR(100) NULL,
    ApplePrivateKey NVARCHAR(MAX) NULL, -- Encrypted
    
    LinkedInEnabled BIT NOT NULL DEFAULT 0,
    LinkedInClientId NVARCHAR(500) NULL,
    LinkedInClientSecret NVARCHAR(500) NULL, -- Encrypted
    
    -- Legacy Database Configuration (for apps with own user tables)
    HasLegacyDatabase BIT NOT NULL DEFAULT 0,
    LegacyDatabaseConnectionString NVARCHAR(MAX) NULL, -- Encrypted
    LegacyDatabaseType NVARCHAR(50) NULL, -- SqlServer, PostgreSQL, MySQL
    LegacyUserTableName NVARCHAR(200) NULL,
    LegacyUserIdColumn NVARCHAR(100) NULL,
    LegacyEmailColumn NVARCHAR(100) NULL,
    LegacyPasswordColumn NVARCHAR(100) NULL,
    LegacyPasswordHashAlgorithm NVARCHAR(50) NULL, -- MD5, SHA256, BCrypt, etc.
    LegacyAdditionalColumnsMapping NVARCHAR(MAX) NULL, -- JSON: {"FirstName": "fname", "LastName": "lname"}
    
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Applications_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
    INDEX IX_Applications_TenantId (TenantId),
    INDEX IX_Applications_ClientId (ClientId)
);

-- =============================================
-- USER MANAGEMENT
-- =============================================

CREATE TABLE Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    
    Email NVARCHAR(256) NOT NULL,
    EmailVerified BIT NOT NULL DEFAULT 0,
    
    -- For native authentication (NULL if only external logins)
    PasswordHash NVARCHAR(500) NULL,
    
    PhoneNumber NVARCHAR(50) NULL,
    PhoneNumberVerified BIT NOT NULL DEFAULT 0,
    
    FirstName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NULL,
    
    -- Security
    SecurityStamp UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(), -- Changes on password reset, invalidates all tokens
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    
    -- Account Status
    IsActive BIT NOT NULL DEFAULT 1,
    LockoutEnabled BIT NOT NULL DEFAULT 1,
    LockoutEnd DATETIME2 NULL,
    AccessFailedCount INT NOT NULL DEFAULT 0,
    
    -- Tracking
    LastLoginAt DATETIME2 NULL,
    LastLoginIp NVARCHAR(45) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_Users_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
    CONSTRAINT UQ_Users_Email_Tenant UNIQUE (TenantId, Email),
    INDEX IX_Users_TenantId (TenantId),
    INDEX IX_Users_Email (Email)
);

-- =============================================
-- EXTERNAL IDENTITY PROVIDERS
-- =============================================

CREATE TABLE ExternalLogins (
    ExternalLoginId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    
    Provider NVARCHAR(50) NOT NULL, -- Google, Apple, LinkedIn
    ProviderUserId NVARCHAR(500) NOT NULL, -- The ID from external provider
    ProviderEmail NVARCHAR(256) NULL,
    ProviderDisplayName NVARCHAR(200) NULL,
    
    LinkedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastUsedAt DATETIME2 NULL,
    
    CONSTRAINT FK_ExternalLogins_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT UQ_ExternalLogins_Provider_UserId UNIQUE (Provider, ProviderUserId),
    INDEX IX_ExternalLogins_UserId (UserId)
);

-- =============================================
-- LEGACY DATABASE USER MAPPINGS
-- =============================================

CREATE TABLE ApplicationUserMappings (
    MappingId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    ApplicationId UNIQUEIDENTIFIER NOT NULL,
    
    -- The user ID in the legacy application's database
    LegacyUserId NVARCHAR(100) NOT NULL,
    
    -- Application-specific metadata (roles, permissions, etc.) as JSON
    Metadata NVARCHAR(MAX) NULL, -- {"roles": ["admin"], "department": "IT"}
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT FK_AppUserMappings_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_AppUserMappings_Applications FOREIGN KEY (ApplicationId) REFERENCES Applications(ApplicationId) ON DELETE CASCADE,
    CONSTRAINT UQ_AppUserMappings_App_LegacyUser UNIQUE (ApplicationId, LegacyUserId),
    INDEX IX_AppUserMappings_UserId (UserId),
    INDEX IX_AppUserMappings_ApplicationId (ApplicationId)
);

-- =============================================
-- REFRESH TOKENS
-- =============================================

CREATE TABLE RefreshTokens (
    RefreshTokenId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    ApplicationId UNIQUEIDENTIFIER NOT NULL,
    
    TokenHash NVARCHAR(500) NOT NULL, -- Hashed token value
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NOT NULL,
    
    -- For rotation
    ParentTokenId UNIQUEIDENTIFIER NULL, -- Previous token in rotation chain
    
    -- Revocation
    IsRevoked BIT NOT NULL DEFAULT 0,
    RevokedAt DATETIME2 NULL,
    RevokedReason NVARCHAR(500) NULL, -- Logout, SecurityStampChanged, AdminRevoked, etc.
    RevokedByIp NVARCHAR(45) NULL,
    
    -- Token reuse detection
    IsUsed BIT NOT NULL DEFAULT 0,
    UsedAt DATETIME2 NULL,
    
    -- Tracking
    CreatedByIp NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    
    CONSTRAINT FK_RefreshTokens_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_RefreshTokens_Applications FOREIGN KEY (ApplicationId) REFERENCES Applications(ApplicationId),
    CONSTRAINT FK_RefreshTokens_Parent FOREIGN KEY (ParentTokenId) REFERENCES RefreshTokens(RefreshTokenId),
    INDEX IX_RefreshTokens_UserId (UserId),
    INDEX IX_RefreshTokens_TokenHash (TokenHash),
    INDEX IX_RefreshTokens_ExpiresAt (ExpiresAt) WHERE IsRevoked = 0,
    INDEX IX_RefreshTokens_Application (ApplicationId, UserId) WHERE IsRevoked = 0
);

-- =============================================
-- ADMIN USERS (for management portal)
-- =============================================

CREATE TABLE TenantAdmins (
    AdminId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NULL, -- NULL for super admins
    
    Email NVARCHAR(256) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    
    FirstName NVARCHAR(100) NULL,
    LastName NVARCHAR(100) NULL,
    
    Role NVARCHAR(50) NOT NULL, -- SuperAdmin, TenantAdmin, ApplicationAdmin
    
    -- Security
    SecurityStamp UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    
    IsActive BIT NOT NULL DEFAULT 1,
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastLoginAt DATETIME2 NULL,
    
    CONSTRAINT FK_TenantAdmins_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
    INDEX IX_TenantAdmins_TenantId (TenantId),
    INDEX IX_TenantAdmins_Email (Email)
);

-- =============================================
-- AUDIT LOGGING
-- =============================================

CREATE TABLE AuditLogs (
    AuditLogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NULL,
    
    EventType NVARCHAR(100) NOT NULL, -- Login, LoginFailed, Logout, TokenIssued, TokenRefreshed, PasswordChanged, etc.
    EventCategory NVARCHAR(50) NOT NULL, -- Authentication, Authorization, Administration, Security
    
    UserId UNIQUEIDENTIFIER NULL,
    UserEmail NVARCHAR(256) NULL,
    
    ApplicationId UNIQUEIDENTIFIER NULL,
    ApplicationName NVARCHAR(200) NULL,
    
    IpAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    
    Success BIT NOT NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    
    AdditionalData NVARCHAR(MAX) NULL, -- JSON for extra context
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    
    INDEX IX_AuditLogs_TenantId_CreatedAt (TenantId, CreatedAt DESC),
    INDEX IX_AuditLogs_UserId (UserId),
    INDEX IX_AuditLogs_EventType (EventType),
    INDEX IX_AuditLogs_Success (Success) WHERE Success = 0
);

-- =============================================
-- EMAIL VERIFICATION & PASSWORD RESET TOKENS
-- =============================================

CREATE TABLE VerificationTokens (
    TokenId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    
    TokenType NVARCHAR(50) NOT NULL, -- EmailVerification, PasswordReset
    TokenHash NVARCHAR(500) NOT NULL,
    
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt DATETIME2 NOT NULL,
    
    IsUsed BIT NOT NULL DEFAULT 0,
    UsedAt DATETIME2 NULL,
    
    CONSTRAINT FK_VerificationTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    INDEX IX_VerificationTokens_TokenHash (TokenHash),
    INDEX IX_VerificationTokens_UserId_Type (UserId, TokenType)
);