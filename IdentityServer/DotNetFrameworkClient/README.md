# AuthServer .NET Framework Client Library

This is a ready-to-use client library for integrating .NET Framework 4.5+ applications with AuthServer.

## Installation

1. Add `AuthServerClient.cs` to your project
2. Install required NuGet packages:

```bash
Install-Package Newtonsoft.Json
Install-Package System.Net.Http
```

## Configuration

Add to your `App.config` or `Web.config`:

```xml
<appSettings>
  <add key="AuthServer:BaseUrl" value="https://localhost:7115/api" />
  <add key="AuthServer:ClientId" value="YOUR_CLIENT_ID" />
  <add key="AuthServer:ClientSecret" value="YOUR_CLIENT_SECRET" />
</appSettings>
```

## Usage Examples

### Basic Login

```csharp
using AuthServerClient;

var client = new AuthServerClient();
var result = await client.LoginAsync("user@example.com", "password");

Console.WriteLine($"Access Token: {result.AccessToken}");
Console.WriteLine($"User: {result.User.FirstName} {result.User.LastName}");

// Store tokens securely
SaveTokens(result.AccessToken, result.RefreshToken);
```

### Refresh Token

```csharp
var newTokens = await client.RefreshTokenAsync(storedRefreshToken);
SaveTokens(newTokens.AccessToken, newTokens.RefreshToken);
```

### Making Authenticated API Calls

```csharp
using (var authenticatedClient = client.CreateAuthenticatedClient(accessToken, refreshToken))
{
    var response = await authenticatedClient.GetAsync("https://your-api.com/api/data");
    var content = await response.Content.ReadAsStringAsync();

    // Token is automatically refreshed if expired
}
```

### Change Password

```csharp
var success = await client.ChangePasswordAsync(
    accessToken,
    "currentPassword",
    "newPassword");
```

### Logout

```csharp
await client.LogoutAsync(refreshToken);
```

## Features

- Automatic token refresh
- Comprehensive error handling
- Support for all authentication flows
- Thread-safe operations
- Disposable pattern for proper resource cleanup
- Custom exceptions with status codes

## Exception Handling

```csharp
try
{
    var result = await client.LoginAsync(email, password);
}
catch (AuthServerException ex)
{
    Console.WriteLine($"Auth error: {ex.Message}");
    if (ex.StatusCode.HasValue)
    {
        Console.WriteLine($"Status code: {ex.StatusCode.Value}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

## License

This client library is provided as part of the AuthServer project.
