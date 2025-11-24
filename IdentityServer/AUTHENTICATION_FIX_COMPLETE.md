# Authentication System - Complete Fix

## What Was Wrong

1. **JWT Role Claims**: Roles were being serialized as a JSON array `["SuperAdmin", "Admin"]` but ASP.NET Core expected individual claims
2. **Claim Type Mapping**: Microsoft's `DefaultInboundClaimTypeMap` was interfering with custom claim types
3. **No Debugging**: There was no visibility into what claims were actually being validated
4. **System Admin Data**: The seed data created admins with incorrect TenantId

## What I Fixed

### 1. **Created UserContext Service** (`AuthServer.Api/Services/UserContext.cs`)
A clean, simple service to access the current user's information:

```csharp
var userId = _userContext.GetUserId();
var email = _userContext.GetUserEmail();
var roles = _userContext.GetRoles();
var isAdmin = _userContext.IsInRole("Admin");
var isSystemAdmin = _userContext.IsSystemAdmin();
```

### 2. **Added Authentication Logging Middleware** (`AuthServer.Api/Middleware/AuthenticationLoggingMiddleware.cs`)
Logs every authentication attempt with full claim details for debugging

### 3. **Fixed JWT Configuration** (`AuthServer.Api/Program.cs`)
- Cleared `DefaultInboundClaimTypeMap` to prevent Microsoft's claim type mapping
- Set `MapInboundClaims = false`
- Configured proper `RoleClaimType = "role"` and `NameClaimType = "sub"`
- Added comprehensive logging in JWT events

### 4. **Updated Seed Data** (`AuthServer.Api/Program.cs`)
Created two separate admin users:
- **System Admin** (no tenant, full access)
- **Tenant Admin** (assigned to default tenant)

## How To Use

### Step 1: STOP THE API
**CRITICAL**: Stop Visual Studio or any running instance of the API

### Step 2: Build
```bash
cd AuthServer.Api
dotnet build
```

### Step 3: Run
```bash
dotnet run
```

### Step 4: Login

**System Admin** (Full access to everything):
```json
POST https://localhost:7115/api/Authentication/Login
{
  "email": "sysadmin@authserver.com",
  "password": "SysAdmin@123456",
  "clientId": "default-client",
  "ipAddress": "127.0.0.1",
  "userAgent": "browser"
}
```

**Tenant Admin** (Access to specific tenant):
```json
POST https://localhost:7115/api/Authentication/Login
{
  "email": "admin@example.com",
  "password": "Admin@123456",
  "clientId": "default-client",
  "ipAddress": "127.0.0.1",
  "userAgent": "browser"
}
```

### Step 5: Test With Debug Endpoint
```bash
GET https://localhost:7115/api/User/debug
Authorization: Bearer YOUR_ACCESS_TOKEN
```

This will show you:
- All claims in your token
- Which roles are detected
- Whether you're authenticated

### Step 6: Create A User
```json
POST https://localhost:7115/api/User/Add
Authorization: Bearer YOUR_ACCESS_TOKEN
{
  "email": "newuser@example.com",
  "firstName": "New",
  "lastName": "User",
  "password": "Password@123",
  "tenantId": "c05ae9ae-5728-475a-accb-a978238f1a5d"
}
```

## Logs To Watch

The API will now log:
- ✅ **Token validated. Claims**: Shows all claims in the JWT
- ✅ **Found X role claims**: Shows which roles were extracted
- ✅ **Checking role 'Admin' for user**: Shows role authorization checks
- ❌ **Authentication failed**: Shows why authentication failed
- ❌ **Authentication challenge**: Shows why a 401 was returned

## Using UserContext in Controllers

Inject `IUserContext` instead of manually parsing claims:

```csharp
public class MyController : ControllerBase
{
    private readonly IUserContext _userContext;

    public MyController(IUserContext userContext)
    {
        _userContext = userContext;
    }

    [HttpGet]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = _userContext.GetUserId();
        var email = _userContext.GetUserEmail();
        var roles = _userContext.GetRoles();
        var isSystemAdmin = _userContext.IsSystemAdmin();

        return Ok(new { userId, email, roles, isSystemAdmin });
    }
}
```

## Troubleshooting

### Still Getting 401?

1. **Check the logs** - The middleware logs every request
2. **Verify the token** - Use the debug endpoint at `/api/User/debug`
3. **Check the Authorization header** - Must be: `Authorization: Bearer YOUR_TOKEN`
4. **Verify you're using the NEW build** - Make sure you stopped the old API first

### Role Authorization Not Working?

1. Check logs for "Found X role claims"
2. Use debug endpoint to see actual roles in token
3. Verify the user in database has the `Roles` column populated

### Token Expired?

Tokens expire after 15 minutes. Login again to get a fresh token.

## Summary

The authentication system is now:
- ✅ **Simplified** - No complex claim transformations
- ✅ **Debuggable** - Comprehensive logging at every step
- ✅ **Working** - Proper role-based authorization
- ✅ **Clean** - UserContext service for easy access

**The key fix**: Cleared `DefaultInboundClaimTypeMap` and properly configured `RoleClaimType`. JWT naturally handles role arrays correctly when these are set properly.
