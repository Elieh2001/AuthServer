// User Models
export class UserDto {
  constructor(data = {}) {
    this.id = data.id || '';
    this.tenantId = data.tenantId || null;
    this.email = data.email || '';
    this.emailVerified = data.emailVerified || false;
    this.firstName = data.firstName || '';
    this.lastName = data.lastName || '';
    this.phoneNumber = data.phoneNumber || '';
    this.isActive = data.isActive || true;
    this.isLockedOut = data.isLockedOut || false;
    this.lockoutEnd = data.lockoutEnd || null;
    this.lastLoginAt = data.lastLoginAt || null;
    this.createdAt = data.createdAt || new Date().toISOString();
  }
}

export class CreateUserDto {
  constructor(data = {}) {
    this.email = data.email || '';
    this.password = data.password || '';
    this.firstName = data.firstName || '';
    this.lastName = data.lastName || '';
    this.phoneNumber = data.phoneNumber || '';
    this.tenantId = data.tenantId || null;
  }
}

export class UpdateUserDto {
  constructor(data = {}) {
    this.firstName = data.firstName;
    this.lastName = data.lastName;
    this.phoneNumber = data.phoneNumber;
    this.isActive = data.isActive;
  }
}

export class LoginRequestDto {
  constructor(data = {}) {
    this.email = data.email || '';
    this.password = data.password || '';
    this.tenantName = data.tenantName || '';
    this.clientId = data.clientId || '';
    this.clientSecret = data.clientSecret || '';
  }
}

export class LoginResponseDto {
  constructor(data = {}) {
    this.accessToken = data.accessToken || '';
    this.refreshToken = data.refreshToken || '';
    this.tokenType = data.tokenType || 'Bearer';
    this.expiresIn = data.expiresIn || 3600;
    this.user = data.user ? new UserInfoDto(data.user) : null;
  }
}

export class UserInfoDto {
  constructor(data = {}) {
    this.id = data.id || '';
    this.email = data.email || '';
    this.firstName = data.firstName || '';
    this.lastName = data.lastName || '';
    this.emailVerified = data.emailVerified || false;
    this.tenantId = data.tenantId || null;
    this.tenantName = data.tenantName || '';
    this.isSystemAdmin = data.isSystemAdmin || false;
  }
}

// Application Models
export class ApplicationDto {
  constructor(data = {}) {
    this.id = data.id || '';
    this.tenantId = data.tenantId || null;
    this.name = data.name || '';
    this.description = data.description || '';
    this.clientId = data.clientId || '';
    this.applicationType = data.applicationType || 'WebApplication';
    this.allowedGrantTypes = data.allowedGrantTypes || '';
    this.allowedScopes = data.allowedScopes || '';
    this.redirectUris = data.redirectUris || '';
    this.googleEnabled = data.googleEnabled || false;
    this.appleEnabled = data.appleEnabled || false;
    this.linkedInEnabled = data.linkedInEnabled || false;
    this.hasLegacyDatabase = data.hasLegacyDatabase || false;
    this.isActive = data.isActive || true;
    this.createdAt = data.createdAt || new Date().toISOString();
  }
}

export class CreateApplicationDto {
  constructor(data = {}) {
    this.name = data.name || '';
    this.description = data.description || '';
    this.applicationType = data.applicationType || 'WebApplication';
    this.allowedGrantTypes = data.allowedGrantTypes || 'authorization_code,refresh_token';
    this.allowedScopes = data.allowedScopes || 'openid,profile,email';
    this.redirectUris = data.redirectUris || '';
    this.postLogoutRedirectUris = data.postLogoutRedirectUris || '';
    this.accessTokenLifetimeSeconds = data.accessTokenLifetimeSeconds || 3600;
    this.refreshTokenLifetimeSeconds = data.refreshTokenLifetimeSeconds || 2592000;
    this.googleEnabled = data.googleEnabled || false;
    this.googleClientId = data.googleClientId || '';
    this.googleClientSecret = data.googleClientSecret || '';
    this.appleEnabled = data.appleEnabled || false;
    this.linkedInEnabled = data.linkedInEnabled || false;
    this.hasLegacyDatabase = data.hasLegacyDatabase || false;
    this.legacyDatabaseConnectionString = data.legacyDatabaseConnectionString || '';
    this.legacyDatabaseType = data.legacyDatabaseType || '';
    this.legacyUserTableName = data.legacyUserTableName || '';
    this.legacyUserIdColumn = data.legacyUserIdColumn || '';
    this.legacyEmailColumn = data.legacyEmailColumn || '';
    this.legacyPasswordColumn = data.legacyPasswordColumn || '';
    this.legacyPasswordHashAlgorithm = data.legacyPasswordHashAlgorithm || '';
  }
}

export class UpdateApplicationDto {
  constructor(data = {}) {
    this.name = data.name;
    this.description = data.description;
    this.redirectUris = data.redirectUris;
    this.googleEnabled = data.googleEnabled;
    this.isActive = data.isActive;
  }
}

// Tenant Models
export class TenantDto {
  constructor(data = {}) {
    this.id = data.id || '';
    this.name = data.name || '';
    this.subdomain = data.subdomain || '';
    this.customDomain = data.customDomain || '';
    this.subscriptionPlan = data.subscriptionPlan || 'Free';
    this.maxUsers = data.maxUsers || null;
    this.maxApplications = data.maxApplications || null;
    this.isActive = data.isActive || true;
    this.subscriptionStartDate = data.subscriptionStartDate || null;
    this.subscriptionEndDate = data.subscriptionEndDate || null;
    this.createdAt = data.createdAt || new Date().toISOString();
  }
}

export class CreateTenantDto {
  constructor(data = {}) {
    this.name = data.name || '';
    this.subdomain = data.subdomain || '';
    this.customDomain = data.customDomain || '';
    this.subscriptionPlan = data.subscriptionPlan || 'Free';
    this.maxUsers = data.maxUsers || null;
    this.maxApplications = data.maxApplications || null;
    this.adminEmail = data.adminEmail || '';
    this.adminPassword = data.adminPassword || '';
    this.adminFirstName = data.adminFirstName || '';
    this.adminLastName = data.adminLastName || '';
  }
}

export class UpdateTenantDto {
  constructor(data = {}) {
    this.name = data.name;
    this.customDomain = data.customDomain;
    this.subscriptionPlan = data.subscriptionPlan;
    this.maxUsers = data.maxUsers;
    this.maxApplications = data.maxApplications;
    this.isActive = data.isActive;
  }
}

// Audit Log Models
export class AuditLogDto {
  constructor(data = {}) {
    this.id = data.id || '';
    this.tenantId = data.tenantId || null;
    this.userId = data.userId || null;
    this.userName = data.userName || '';
    this.action = data.action || '';
    this.entityType = data.entityType || '';
    this.entityId = data.entityId || '';
    this.changes = data.changes || '';
    this.ipAddress = data.ipAddress || '';
    this.userAgent = data.userAgent || '';
    this.additionalData = data.additionalData || {};
    this.timestamp = data.timestamp || new Date().toISOString();
  }
}

// Result Models
export class Result {
  constructor(data = {}) {
    this.success = data.success || false;
    this.message = data.message || '';
    this.data = data.data || null;
    this.errors = data.errors || [];
  }
}

export class PagedResult {
  constructor(data = {}) {
    this.items = data.items || [];
    this.totalCount = data.totalCount || 0;
    this.pageNumber = data.pageNumber || 1;
    this.pageSize = data.pageSize || 10;
    this.totalPages = data.totalPages || 0;
  }
}

// Token Models
export class RefreshTokenRequest {
  constructor(data = {}) {
    this.refreshToken = data.refreshToken || '';
    this.clientId = data.clientId || '';
    this.clientSecret = data.clientSecret || '';
  }
}
