# .NET Framework 4.5+ Integration Guide
## Connecting Your .NET Framework Application to AuthServer

This guide demonstrates how to integrate a .NET Framework 4.5+ application (Web API, MVC, or WinForms) with the AuthServer for authentication and authorization.

## Table of Contents
1. [Prerequisites](#prerequisites)
2. [Quick Start](#quick-start)
3. [Setup Instructions](#setup-instructions)
4. [Authentication Flow](#authentication-flow)
5. [Securing API Endpoints](#securing-api-endpoints)
6. [Client Library](#client-library)
7. [Example Implementations](#example-implementations)

---

## Prerequisites

- .NET Framework 4.5 or higher
- Visual Studio 2015 or higher
- NuGet Package Manager
- Application registered in AuthServer (you'll need ClientId and ClientSecret)

## Quick Start

### 1. Register Your Application in AuthServer

First, create an application in the AuthServer UI:

```
Navigate to Applications > Create New Application

Fill in:
- Name: "My Legacy App"
- Application Type: "WebApplication" or "NativeApplication"
- Redirect URIs: "http://localhost:YOUR_PORT/api/auth/callback"
- Allowed Grant Types: "authorization_code,refresh_token,client_credentials"
- Allowed Scopes: "openid,profile,email"

Save the ClientId and ClientSecret - you'll need these!
```

### 2. Install Required NuGet Packages

```bash
Install-Package System.IdentityModel.Tokens.Jwt
Install-Package Microsoft.Owin.Security.Jwt
Install-Package Microsoft.Owin.Host.SystemWeb
Install-Package Newtonsoft.Json
Install-Package System.Net.Http
```

### 3. Configure Web.config

Add the following to your `<appSettings>` section:

```xml
<appSettings>
  <add key="AuthServer:BaseUrl" value="https://localhost:7115/api" />
  <add key="AuthServer:ClientId" value="YOUR_CLIENT_ID" />
  <add key="AuthServer:ClientSecret" value="YOUR_CLIENT_SECRET" />
  <add key="AuthServer:JwtSecret" value="YOUR_JWT_SECRET_FROM_AUTHSERVER" />
  <add key="AuthServer:Issuer" value="AuthServer" />
  <add key="AuthServer:Audience" value="AuthServer" />
</appSettings>
```

---

## Setup Instructions

### Step 1: Create OWIN Startup Class

Create a new file `Startup.cs` in your project root:

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
                    TokenValidationParameters = new TokenValidationParameters()
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

### Step 2: Create AuthServer Client Library

Create a folder `AuthServer` and add `AuthServerClient.cs`:

```csharp
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace YourNamespace.AuthServer
{
    public class AuthServerClient
    {
        private readonly string _baseUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly HttpClient _httpClient;

        public AuthServerClient()
        {
            _baseUrl = ConfigurationManager.AppSettings["AuthServer:BaseUrl"];
            _clientId = ConfigurationManager.AppSettings["AuthServer:ClientId"];
            _clientSecret = ConfigurationManager.AppSettings["AuthServer:ClientSecret"];

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var loginRequest = new
            {
                email = email,
                password = password,
                clientId = _clientId,
                clientSecret = _clientSecret
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(loginRequest),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/Authentication/Login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Login failed: {responseContent}");
            }

            var result = JsonConvert.DeserializeObject<ApiResult<LoginResponse>>(responseContent);

            if (!result.Success)
            {
                throw new Exception(result.Message);
            }

            return result.Data;
        }

        public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
        {
            var refreshRequest = new
            {
                refreshToken = refreshToken,
                clientId = _clientId,
                clientSecret = _clientSecret
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(refreshRequest),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/Authentication/RefreshToken", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Token refresh failed: {responseContent}");
            }

            var result = JsonConvert.DeserializeObject<ApiResult<LoginResponse>>(responseContent);

            if (!result.Success)
            {
                throw new Exception(result.Message);
            }

            return result.Data;
        }

        public async Task<bool> ValidateTokenAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync("/Authentication/ValidateToken");
            return response.IsSuccessStatusCode;
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var logoutRequest = new { refreshToken = refreshToken };

            var content = new StringContent(
                JsonConvert.SerializeObject(logoutRequest),
                Encoding.UTF8,
                "application/json");

            await _httpClient.PostAsync("/Authentication/Logout", content);
        }
    }

    #region DTOs

    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public UserInfo User { get; set; }
    }

    public class UserInfo
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool EmailVerified { get; set; }
        public Guid? TenantId { get; set; }
        public string TenantName { get; set; }
        public bool IsSystemAdmin { get; set; }
    }

    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    #endregion
}
```

---

## Authentication Flow

### For Web API / MVC Applications

#### 1. Login Endpoint

Create an authentication controller:

```csharp
using System;
using System.Threading.Tasks;
using System.Web.Http;
using YourNamespace.AuthServer;

namespace YourNamespace.Controllers
{
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly AuthServerClient _authClient;

        public AuthController()
        {
            _authClient = new AuthServerClient();
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
        public async Task<IHttpActionResult> RefreshToken(RefreshTokenRequest request)
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
            try
            {
                await _authClient.LogoutAsync(request.RefreshToken);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; }
    }

    public class LogoutRequest
    {
        public string RefreshToken { get; set; }
    }
}
```

---

## Securing API Endpoints

### Using the [Authorize] Attribute

Simply add the `[Authorize]` attribute to controllers or actions:

```csharp
using System.Web.Http;

namespace YourNamespace.Controllers
{
    [Authorize] // Secure entire controller
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            // This endpoint requires authentication
            var userId = User.Identity.Name;
            return Ok(new { message = $"Hello {userId}" });
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "Admin")] // Require specific role
        public IHttpActionResult Create(ProductDto product)
        {
            // Only users with Admin role can access
            return Ok();
        }

        [HttpGet]
        [Route("{id}")]
        [AllowAnonymous] // Allow anonymous access to specific endpoint
        public IHttpActionResult Get(int id)
        {
            // This endpoint is public
            return Ok();
        }
    }
}
```

### Custom Authorization Filter

Create a custom authorization attribute for more control:

```csharp
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace YourNamespace.Filters
{
    public class AuthServerAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] _requiredRoles;
        private readonly string[] _requiredScopes;

        public AuthServerAuthorizeAttribute(string roles = null, string scopes = null)
        {
            _requiredRoles = roles?.Split(',') ?? new string[0];
            _requiredScopes = scopes?.Split(',') ?? new string[0];
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (!base.IsAuthorized(actionContext))
                return false;

            var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;
            if (principal == null)
                return false;

            // Check roles
            if (_requiredRoles.Any())
            {
                var userRoles = principal.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value);

                if (!_requiredRoles.Any(r => userRoles.Contains(r)))
                    return false;
            }

            // Check scopes
            if (_requiredScopes.Any())
            {
                var userScopes = principal.Claims
                    .Where(c => c.Type == "scope")
                    .Select(c => c.Value);

                if (!_requiredScopes.Any(s => userScopes.Contains(s)))
                    return false;
            }

            return true;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.Forbidden,
                new { message = "Access denied. Insufficient permissions." });
        }
    }
}

// Usage:
[AuthServerAuthorize(roles: "Admin,Manager", scopes: "write")]
public IHttpActionResult CreateProduct(ProductDto product)
{
    return Ok();
}
```

---

## Client Library

### HttpClient Helper with Automatic Token Refresh

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using YourNamespace.AuthServer;

namespace YourNamespace.Helpers
{
    public class AuthenticatedHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthServerClient _authClient;
        private string _accessToken;
        private string _refreshToken;

        public AuthenticatedHttpClient(string accessToken, string refreshToken)
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _authClient = new AuthServerClient();
            _httpClient = new HttpClient();
            UpdateAuthorizationHeader();
        }

        private void UpdateAuthorizationHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await RefreshTokenAsync();
                response = await _httpClient.GetAsync(url);
            }

            return response;
        }

        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            var response = await _httpClient.PostAsync(url, content);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await RefreshTokenAsync();
                response = await _httpClient.PostAsync(url, content);
            }

            return response;
        }

        private async Task RefreshTokenAsync()
        {
            var result = await _authClient.RefreshTokenAsync(_refreshToken);
            _accessToken = result.AccessToken;
            _refreshToken = result.RefreshToken;
            UpdateAuthorizationHeader();
        }

        public string AccessToken => _accessToken;
        public string RefreshToken => _refreshToken;
    }
}
```

---

## Example Implementations

### Example 1: WinForms Desktop Application

```csharp
using System;
using System.Windows.Forms;
using YourNamespace.AuthServer;

namespace YourNamespace.Desktop
{
    public partial class LoginForm : Form
    {
        private readonly AuthServerClient _authClient;

        public LoginForm()
        {
            InitializeComponent();
            _authClient = new AuthServerClient();
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                var result = await _authClient.LoginAsync(
                    txtEmail.Text,
                    txtPassword.Text);

                // Store tokens
                Properties.Settings.Default.AccessToken = result.AccessToken;
                Properties.Settings.Default.RefreshToken = result.RefreshToken;
                Properties.Settings.Default.Save();

                MessageBox.Show($"Welcome {result.User.FirstName}!");

                // Open main form
                var mainForm = new MainForm();
                mainForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}", "Error");
            }
        }
    }
}
```

### Example 2: Web API with Claims

```csharp
using System.Linq;
using System.Security.Claims;
using System.Web.Http;

namespace YourNamespace.Controllers
{
    [Authorize]
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        [HttpGet]
        [Route("profile")]
        public IHttpActionResult GetProfile()
        {
            var principal = User as ClaimsPrincipal;

            var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = principal?.FindFirst(ClaimTypes.Email)?.Value;
            var firstName = principal?.FindFirst(ClaimTypes.GivenName)?.Value;
            var lastName = principal?.FindFirst(ClaimTypes.Surname)?.Value;
            var tenantId = principal?.FindFirst("tenant_id")?.Value;
            var isAdmin = principal?.IsInRole("Admin") ?? false;

            return Ok(new
            {
                userId,
                email,
                firstName,
                lastName,
                tenantId,
                isAdmin
            });
        }
    }
}
```

### Example 3: MVC Application

```csharp
using System.Web.Mvc;

namespace YourNamespace.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            var userName = User.Identity.Name;
            ViewBag.UserName = userName;
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminPanel()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }
    }
}
```

---

## Testing Your Integration

### 1. Test Login

```csharp
var client = new AuthServerClient();
var result = await client.LoginAsync("user@example.com", "password");
Console.WriteLine($"Access Token: {result.AccessToken}");
Console.WriteLine($"User: {result.User.FirstName} {result.User.LastName}");
```

### 2. Test Protected Endpoint

```csharp
var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", accessToken);

var response = await httpClient.GetAsync("http://localhost:YOUR_PORT/api/products");
var content = await response.Content.ReadAsStringAsync();
Console.WriteLine(content);
```

### 3. Test Token Refresh

```csharp
var newTokens = await client.RefreshTokenAsync(refreshToken);
Console.WriteLine($"New Access Token: {newTokens.AccessToken}");
```

---

## Troubleshooting

### Common Issues

1. **401 Unauthorized**
   - Check if the JWT secret matches between AuthServer and your app
   - Verify the token hasn't expired
   - Ensure the Authorization header is properly formatted: `Bearer {token}`

2. **CORS Issues**
   - Add CORS configuration in Web.config or enable it in your Web API configuration

3. **Token Validation Fails**
   - Verify issuer and audience match the configuration
   - Check clock skew settings
   - Ensure the JWT secret is correct

### Enable Logging

Add to Web.config:

```xml
<system.diagnostics>
  <trace autoflush="true" indentsize="4">
    <listeners>
      <add name="myListener"
           type="System.Diagnostics.TextWriterTraceListener"
           initializeData="AuthLog.txt" />
    </listeners>
  </trace>
</system.diagnostics>
```

---

## Security Best Practices

1. **Never store secrets in code** - Use Web.config transformations for different environments
2. **Use HTTPS** - Always use SSL/TLS in production
3. **Store tokens securely** - Use encrypted storage for desktop apps
4. **Implement token refresh** - Refresh tokens before they expire
5. **Validate all claims** - Don't trust client-provided data
6. **Use short-lived access tokens** - Recommended: 15-60 minutes
7. **Rotate refresh tokens** - Enable refresh token rotation in AuthServer

---

## Additional Resources

- JWT.io - Token debugger
- OWIN Documentation
- OAuth 2.0 RFC 6749
- JWT RFC 7519

---

## Support

For issues specific to AuthServer integration, check the AuthServer documentation or raise an issue on the project repository.
