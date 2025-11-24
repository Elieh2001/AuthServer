# AuthServer - Complete Setup and Run Guide

This guide will help you set up and run the AuthServer solution and integrate it with your .NET Framework application.

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Initial Setup](#initial-setup)
3. [Database Configuration](#database-configuration)
4. [Running the Solution](#running-the-solution)
5. [Creating Your First Application](#creating-your-first-application)
6. [Integrating with .NET Framework](#integrating-with-net-framework)
7. [Testing the Integration](#testing-the-integration)

---

## Prerequisites

### Required Software
- .NET 9.0 SDK
- SQL Server 2019+ or SQL Server Express
- Node.js 18+ and npm
- Visual Studio 2022 or VS Code
- Postman or similar API testing tool (optional)

### Check Versions

```bash
dotnet --version  # Should be 9.0.x
node --version    # Should be 18+
npm --version     # Should be 9+
```

---

## Initial Setup

### Step 1: Clone/Navigate to the Project

```bash
cd C:\Users\es\Desktop\IdentityServer\AuthServer\IdentityServer
```

### Step 2: Install Dependencies

#### Backend (.NET)
```bash
dotnet restore
dotnet build
```

#### Frontend (React)
```bash
cd AuthServer.UI
npm install
cd ..
```

---

## Database Configuration

### Step 1: Update Connection String

Edit `AuthServer.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=AuthServerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

For SQL Server Express:
```
Server=.\\SQLEXPRESS;Database=AuthServerDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

For SQL Server with credentials:
```
Server=YOUR_SERVER;Database=AuthServerDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true
```

### Step 2: Configure JWT Settings

In `appsettings.json`, update the JWT secret (use a strong random string):

```json
{
  "Jwt": {
    "Secret": "YOUR_SUPER_SECRET_KEY_AT_LEAST_32_CHARACTERS_LONG_12345",
    "Issuer": "AuthServer",
    "Audience": "AuthServer",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30
  }
}
```

### Step 3: Create Database Migration

```bash
# Remove old migrations (if any errors occurred)
dotnet ef migrations remove --project AuthServer.Infrastructure --startup-project AuthServer.Api

# Create new migration
dotnet ef migrations add InitialCreate --project AuthServer.Infrastructure --startup-project AuthServer.Api

# Apply migration to database
dotnet ef database update --project AuthServer.Infrastructure --startup-project AuthServer.Api
```

### Step 4: Verify Database Creation

Connect to your SQL Server and verify the `AuthServerDb` database was created with the following tables:
- Applications
- Users
- Tenants
- TenantApplications
- TenantAdmins
- RefreshTokens
- ExternalLogins
- ApplicationUserMappings
- VerificationTokens
- AuditLogs

---

## Running the Solution

### Option 1: Run Backend and Frontend Separately

#### Terminal 1 - Backend API
```bash
cd AuthServer.Api
dotnet run
```

The API will start at:
- HTTPS: `https://localhost:7115`
- HTTP: `http://localhost:5115`

#### Terminal 2 - Frontend UI
```bash
cd AuthServer.UI
npm run dev
```

The UI will start at:
- `http://localhost:5173`

### Option 2: Run with Visual Studio

1. Open `IdentityServer.sln` in Visual Studio
2. Set `AuthServer.Api` as the startup project
3. Press F5 to run

In a separate terminal:
```bash
cd AuthServer.UI
npm run dev
```

### Verify Everything is Running

1. **Backend**: Navigate to `https://localhost:7115/swagger`
   - You should see the Swagger UI with all API endpoints

2. **Frontend**: Navigate to `http://localhost:5173`
   - You should see the login page

---

## Creating Your First Application

### Step 1: Create a System Admin Account

Since this is a fresh installation, you'll need to create the first admin account directly in the database:

```sql
USE AuthServerDb;

-- Create a system admin user
INSERT INTO Users (Id, Email, EmailVerified, PasswordHash, FirstName, LastName, SecurityStamp, IsSystemAdmin, IsActive, CreatedAt, UpdatedAt)
VALUES (
    NEWID(),
    'admin@authserver.com',
    1,
    '$2a$11$YourBcryptHashedPasswordHere', -- Use BCrypt to hash 'Admin@123'
    'System',
    'Admin',
    NEWID(),
    1,
    1,
    GETUTCDATE(),
    GETUTCDATE()
);
```

**Better Option**: Use the API endpoint to create the first user via Postman:

```
POST https://localhost:7115/api/Authentication/Register
Content-Type: application/json

{
  "email": "admin@authserver.com",
  "password": "Admin@123!",
  "firstName": "System",
  "lastName": "Admin",
  "tenantId": null
}
```

### Step 2: Login to the UI

1. Navigate to `http://localhost:5173`
2. Login with `admin@authserver.com` and the password you set
3. You'll be redirected to the dashboard

### Step 3: Create an Application

1. Go to **Applications** > **Create New**
2. Fill in the details:
   ```
   Name: My Legacy App
   Description: .NET Framework 4.5 Application
   Application Type: WebApplication
   Allowed Grant Types: authorization_code,refresh_token,client_credentials
   Allowed Scopes: openid,profile,email
   Redirect URIs: http://localhost:YOUR_APP_PORT/api/auth/callback
   Access Token Lifetime: 3600 (1 hour)
   Refresh Token Lifetime: 2592000 (30 days)
   ```
3. Click **Create**
4. **IMPORTANT**: Copy the `Client Secret` that appears - you won't see it again!

### Step 4: Note Your Credentials

Save these values - you'll need them for integration:
- **Client ID**: (shown in the UI)
- **Client Secret**: (copied after creation)
- **JWT Secret**: (from appsettings.json)

---

## Integrating with .NET Framework

### Step 1: Create or Open Your .NET Framework Project

```bash
# If creating new project
cd C:\Projects
mkdir MyLegacyApp
cd MyLegacyApp
# Create a new Web API project in Visual Studio
```

### Step 2: Add AuthServer Client Library

Copy the files from the AuthServer solution:
```
C:\Users\es\Desktop\IdentityServer\AuthServer\IdentityServer\DotNetFrameworkClient\
```

to your project:
```
C:\Projects\MyLegacyApp\AuthServer\
```

Add the file to your Visual Studio project.

### Step 3: Install Required NuGet Packages

```bash
Install-Package System.IdentityModel.Tokens.Jwt
Install-Package Microsoft.Owin.Security.Jwt
Install-Package Microsoft.Owin.Host.SystemWeb
Install-Package Newtonsoft.Json
Install-Package System.Net.Http
```

### Step 4: Configure Web.config

Add to `<appSettings>`:

```xml
<appSettings>
  <!-- AuthServer Configuration -->
  <add key="AuthServer:BaseUrl" value="https://localhost:7115/api" />
  <add key="AuthServer:ClientId" value="YOUR_CLIENT_ID_FROM_STEP_3" />
  <add key="AuthServer:ClientSecret" value="YOUR_CLIENT_SECRET_FROM_STEP_3" />
  <add key="AuthServer:JwtSecret" value="YOUR_JWT_SECRET_FROM_APPSETTINGS" />
  <add key="AuthServer:Issuer" value="AuthServer" />
  <add key="AuthServer:Audience" value="AuthServer" />
</appSettings>
```

### Step 5: Create OWIN Startup Class

Create `Startup.cs`:

```csharp
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Jwt;
using Microsoft.IdentityModel.Tokens;
using Owin;
using System;
using System.Configuration;
using System.Text;

[assembly: OwinStartup(typeof(YourNamespace.Startup))]

namespace YourNamespace
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            var issuer = ConfigurationManager.AppSettings["AuthServer:Issuer"];
            var audience = ConfigurationManager.AppSettings["AuthServer:Audience"];
            var secret = ConfigurationManager.AppSettings["AuthServer:JwtSecret"];

            var secretKey = Encoding.UTF8.GetBytes(secret);

            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(5)
                    }
                });
        }
    }
}
```

### Step 6: Create Authentication Controller

```csharp
using System;
using System.Threading.Tasks;
using System.Web.Http;
using AuthServerClient;

namespace YourNamespace.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly AuthServerClient.AuthServerClient _authClient;

        public AuthController()
        {
            _authClient = new AuthServerClient.AuthServerClient();
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Login(LoginRequest request)
        {
            try
            {
                var result = await _authClient.LoginAsync(request.Email, request.Password);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("refresh")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> RefreshToken(RefreshRequest request)
        {
            try
            {
                var result = await _authClient.RefreshTokenAsync(request.RefreshToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized();
            }
        }

        [HttpPost]
        [Route("logout")]
        [Authorize]
        public async Task<IHttpActionResult> Logout(LogoutRequest request)
        {
            await _authClient.LogoutAsync(request.RefreshToken);
            return Ok();
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RefreshRequest
    {
        public string RefreshToken { get; set; }
    }

    public class LogoutRequest
    {
        public string RefreshToken { get; set; }
    }
}
```

### Step 7: Secure Your API Endpoints

```csharp
using System.Web.Http;

namespace YourNamespace.Controllers
{
    [Authorize] // Require authentication for all endpoints
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            var userId = User.Identity.Name;
            // Your business logic here
            return Ok(new { message = $"Hello {userId}" });
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "Admin")] // Require Admin role
        public IHttpActionResult Create(ProductDto product)
        {
            // Your business logic here
            return Ok();
        }
    }

    public class ProductDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
```

---

## Testing the Integration

### Step 1: Start Both Applications

1. Start AuthServer (API + UI)
2. Start your .NET Framework application

### Step 2: Test Login

Using Postman:

```
POST http://localhost:YOUR_APP_PORT/api/auth/login
Content-Type: application/json

{
  "email": "admin@authserver.com",
  "password": "Admin@123!"
}
```

Response:
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "abc123...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "id": "...",
    "email": "admin@authserver.com",
    "firstName": "System",
    "lastName": "Admin",
    "isSystemAdmin": true
  }
}
```

### Step 3: Test Protected Endpoint

```
GET http://localhost:YOUR_APP_PORT/api/products
Authorization: Bearer {accessToken}
```

Response:
```json
{
  "message": "Hello admin@authserver.com"
}
```

### Step 4: Test Token Refresh

```
POST http://localhost:YOUR_APP_PORT/api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "abc123..."
}
```

---

## Troubleshooting

### Database Connection Issues

```bash
# Test connection string
dotnet ef database drop --project AuthServer.Infrastructure --startup-project AuthServer.Api
dotnet ef database update --project AuthServer.Infrastructure --startup-project AuthServer.Api
```

### JWT Token Issues

1. Verify the JWT secret is the same in both applications
2. Check token expiration
3. Verify issuer and audience match
4. Use https://jwt.io to debug the token

### CORS Issues

Add to `AuthServer.Api/Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:YOUR_PORT")
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

// After building the app
app.UseCors();
```

### Port Conflicts

Change ports in:
- **Backend**: `AuthServer.Api/Properties/launchSettings.json`
- **Frontend**: `AuthServer.UI/vite.config.js`

---

## Next Steps

1. **Configure Email**: Set up SMTP for password reset emails
2. **Add Tenants**: Create multi-tenant structure
3. **Configure External Providers**: Set up Google/Apple/LinkedIn OAuth
4. **Custom Claims**: Add custom claims to JWT tokens
5. **Audit Logs**: Monitor authentication events
6. **Production Deployment**: Deploy to Azure/AWS/IIS

---

## Security Checklist

- [ ] Change default JWT secret to a strong random value
- [ ] Use HTTPS in production
- [ ] Store secrets in environment variables or Azure Key Vault
- [ ] Enable refresh token rotation
- [ ] Set appropriate token expiration times
- [ ] Implement rate limiting
- [ ] Enable audit logging
- [ ] Regular security updates
- [ ] Use strong password policies

---

## Support and Documentation

- **Integration Guide**: `DOTNET_FRAMEWORK_INTEGRATION_GUIDE.md`
- **Client Library**: `DotNetFrameworkClient/AuthServerClient.cs`
- **Swagger API Docs**: `https://localhost:7115/swagger`

For issues and questions, check the project documentation or create an issue on the repository.
