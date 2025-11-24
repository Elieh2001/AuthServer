namespace AuthServer.Api.Middleware;

public class AuthenticationLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationLoggingMiddleware> _logger;

    public AuthenticationLoggingMiddleware(RequestDelegate next, ILogger<AuthenticationLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log authentication details for every request
        var path = context.Request.Path;
        var method = context.Request.Method;

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var hasAuthHeader = !string.IsNullOrEmpty(authHeader);

        _logger.LogDebug("Request: {Method} {Path} | HasAuthHeader: {HasAuth}",
            method, path, hasAuthHeader);

        await _next(context);

        // Log after authentication
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var claims = context.User.Claims.Select(c => $"{c.Type}={c.Value}");
            _logger.LogDebug("Authenticated User Claims: {Claims}", string.Join(", ", claims));

            var roles = context.User.FindAll("role").Select(c => c.Value).ToList();
            _logger.LogDebug("User Roles: {Roles}", string.Join(", ", roles));
        }
        else if (hasAuthHeader && context.Response.StatusCode == 401)
        {
            _logger.LogWarning("Authentication failed for {Method} {Path}. Status: 401", method, path);
        }
    }
}
