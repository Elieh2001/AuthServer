# AuthServer - Multi-Tenant Authentication Server

A comprehensive authentication and authorization server built with .NET 9.0, featuring multi-tenancy, JWT tokens, external OAuth providers, and legacy database integration.

## Features

- **Multi-Tenant Architecture**: Isolate users and applications by tenant
- **JWT Authentication**: Secure token-based authentication with refresh tokens
- **External OAuth Providers**: Google, Apple, LinkedIn integration
- **Legacy Database Support**: Migrate users from existing databases
- **Audit Logging**: Track all authentication events
- **React Admin UI**: Modern web interface for management
- **.NET Framework Client**: Ready-to-use library for .NET Framework 4.5+ apps
- **RESTful API**: Comprehensive API with Swagger documentation

## Quick Start

### 1. Prerequisites
- .NET 9.0 SDK
- SQL Server 2019+
- Node.js 18+

### 2. Setup Database

```bash
# Update connection string in AuthServer.Api/appsettings.json
# Then run migrations
dotnet ef database update --project AuthServer.Infrastructure --startup-project AuthServer.Api
```

### 3. Run the Solution

```bash
# Terminal 1 - Backend API
cd AuthServer.Api
dotnet run

# Terminal 2 - Frontend UI
cd AuthServer.UI
npm install
npm run dev
```

### 4. Access the Application

- **API**: https://localhost:7115
- **Swagger**: https://localhost:7115/swagger
- **UI**: http://localhost:5173

## Documentation

- **[Setup and Run Guide](./SETUP_AND_RUN_GUIDE.md)** - Complete setup instructions
- **[.NET Framework Integration](./DOTNET_FRAMEWORK_INTEGRATION_GUIDE.md)** - How to integrate with legacy apps
- **[Client Library](./DotNetFrameworkClient/README.md)** - Ready-to-use client for .NET Framework

## Architecture

```
AuthServer/
├── AuthServer.Api/              # Web API project
├── AuthServer.Application/      # Business logic layer
├── AuthServer.Domain/           # Domain entities and interfaces
├── AuthServer.Infrastructure/   # Data access and EF Core
├── AuthServer.UI/               # React admin interface
└── DotNetFrameworkClient/       # Client library for .NET Framework
```

## Key Technologies

- **.NET 9.0**: Modern C# with Web API
- **Entity Framework Core 9**: ORM with SQL Server
- **JWT**: JSON Web Tokens for authentication
- **React 18**: Modern UI framework
- **Vite**: Fast frontend build tool
- **BCrypt**: Secure password hashing
- **Swagger**: API documentation

## Features Breakdown

### Authentication
- ✅ Email/Password login
- ✅ JWT access tokens with refresh tokens
- ✅ Token rotation for enhanced security
- ✅ Password reset via email
- ✅ Email verification
- ✅ External OAuth (Google, Apple, LinkedIn)

### Authorization
- ✅ Role-based access control
- ✅ Scope-based permissions
- ✅ Tenant isolation
- ✅ Application-level access control

### Multi-Tenancy
- ✅ Tenant management
- ✅ Subdomain support
- ✅ Custom domain support
- ✅ Subscription plans
- ✅ Tenant-level application assignments

### Management
- ✅ User management (CRUD)
- ✅ Application management (OAuth clients)
- ✅ Tenant management
- ✅ Audit log viewing
- ✅ Token management

## API Endpoints

### Authentication
- `POST /api/Authentication/Login` - User login
- `POST /api/Authentication/Register` - User registration
- `POST /api/Authentication/RefreshToken` - Refresh access token
- `POST /api/Authentication/Logout` - Logout user
- `POST /api/Authentication/ChangePassword` - Change password
- `POST /api/Authentication/RequestPasswordReset` - Request password reset
- `POST /api/Authentication/ResetPassword` - Reset password
- `POST /api/Authentication/VerifyEmail` - Verify email address

### Users
- `GET /api/User` - Get all users (paginated)
- `GET /api/User/{id}` - Get user by ID
- `GET /api/User/email/{email}` - Get user by email
- `GET /api/User/tenant/{tenantId}` - Get users by tenant
- `POST /api/User` - Create user
- `PUT /api/User/{id}` - Update user
- `DELETE /api/User/{id}` - Delete user
- `POST /api/User/{id}/unlock` - Unlock user account

### Applications
- `GET /api/Application` - Get all applications
- `GET /api/Application/{id}` - Get application by ID
- `GET /api/Application/client/{clientId}` - Get by client ID
- `GET /api/Application/tenant/{tenantId}` - Get applications by tenant
- `POST /api/Application` - Create application
- `PUT /api/Application/{id}` - Update application
- `DELETE /api/Application/{id}` - Delete application
- `POST /api/Application/{id}/regenerate-secret` - Regenerate client secret

### Tenants
- `GET /api/Tenant` - Get all tenants (paginated)
- `GET /api/Tenant/{id}` - Get tenant by ID
- `GET /api/Tenant/subdomain/{subdomain}` - Get by subdomain
- `POST /api/Tenant` - Create tenant
- `PUT /api/Tenant/{id}` - Update tenant
- `DELETE /api/Tenant/{id}` - Delete tenant
- `POST /api/Tenant/{id}/activate` - Activate tenant
- `POST /api/Tenant/{id}/deactivate` - Deactivate tenant

### Audit Logs
- `GET /api/Audit` - Get all audit logs (paginated)
- `GET /api/Audit/tenant/{tenantId}` - Get logs by tenant
- `GET /api/Audit/user/{userId}` - Get logs by user
- `GET /api/Audit/action/{action}` - Get logs by action
- `GET /api/Audit/daterange` - Get logs by date range

## .NET Framework Integration

### Installation

1. Add `DotNetFrameworkClient/AuthServerClient.cs` to your project
2. Install NuGet packages:
   ```
   Install-Package Newtonsoft.Json
   Install-Package Microsoft.Owin.Security.Jwt
   Install-Package Microsoft.Owin.Host.SystemWeb
   ```

### Usage

```csharp
using AuthServerClient;

// Login
var client = new AuthServerClient();
var result = await client.LoginAsync("user@example.com", "password");

// Make authenticated calls
using (var authClient = client.CreateAuthenticatedClient(
    result.AccessToken,
    result.RefreshToken))
{
    var response = await authClient.GetAsync("https://your-api.com/api/data");
    // Token automatically refreshes if expired
}
```

See **[Integration Guide](./DOTNET_FRAMEWORK_INTEGRATION_GUIDE.md)** for complete details.

## Security Features

- **BCrypt Password Hashing**: Industry-standard password security
- **JWT Tokens**: Secure stateless authentication
- **Refresh Token Rotation**: Enhanced token security
- **Token Expiration**: Configurable token lifetimes
- **Account Lockout**: Protect against brute force attacks
- **Email Verification**: Confirm user email addresses
- **Audit Logging**: Track all security events
- **HTTPS Enforcement**: Secure communication
- **SQL Injection Protection**: Parameterized queries via EF Core
- **XSS Protection**: Input validation and output encoding

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=AuthServerDb;Trusted_Connection=True;"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-at-least-32-characters",
    "Issuer": "AuthServer",
    "Audience": "AuthServer",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@authserver.com",
    "FromName": "AuthServer"
  }
}
```

## Development

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Create Migration

```bash
dotnet ef migrations add MigrationName --project AuthServer.Infrastructure --startup-project AuthServer.Api
```

### Update Database

```bash
dotnet ef database update --project AuthServer.Infrastructure --startup-project AuthServer.Api
```

## Deployment

### Requirements
- Windows Server or Linux with .NET 9.0 runtime
- SQL Server database
- HTTPS certificate
- SMTP server for emails

### Steps
1. Update connection strings and JWT secrets
2. Build for release: `dotnet publish -c Release`
3. Deploy to IIS, Azure App Service, or Docker
4. Configure environment variables
5. Run database migrations
6. Configure DNS and SSL

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License.

## Support

- **Documentation**: See guides in the root directory
- **Issues**: Create an issue on the repository
- **API Docs**: Access Swagger at `/swagger` when running

---

**Built with ❤️ using .NET 9.0 and React**
