using System.Security.Claims;
using System.Text;
using AuthServer.Application.Interfaces;
using AuthServer.Application.Services;
using AuthServer.Domain.Interfaces;
using AuthServer.Infrastructure.Data;
using AuthServer.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;

var builder = WebApplication.CreateBuilder(args);

// ===============================================
// 1. CONTROLLERS & API
// ===============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// ===============================================
// USER CONTEXT SERVICE
// ===============================================
builder.Services.AddScoped<AuthServer.Api.Services.IUserContext, AuthServer.Api.Services.UserContext>();

// ===============================================
// 2. SWAGGER CONFIGURATION
// ===============================================
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auth Server API",
        Version = "v1",
        Description = "Multi-Tenant Authentication and Authorization Server"
    });

    // JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ===============================================
// 3. DATABASE CONFIGURATION
// ===============================================
builder.Services.AddDbContext<AuthServerDbContext>((serviceProvider, options) =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(

            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)
    );

    // For runtime, you could resolve tenant from HttpContext
    // For now, we'll pass null to disable tenant filtering during startup
});

// ===============================================
// 4. DATA PROTECTION (For Encryption)
// ===============================================
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("./keys"))
    .SetApplicationName("AuthServer");

// ===============================================
// 5. REPOSITORY & UNIT OF WORK
// ===============================================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ===============================================
// 6. APPLICATION SERVICES
// ===============================================
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<ILegacyAuthenticationService, LegacyAuthenticationService>();

// ===============================================
// 7. JWT AUTHENTICATION CONFIGURATION
// ===============================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var key = Encoding.UTF8.GetBytes(secretKey);

// Configure JWT Authentication with proper role handling
System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(int.Parse(jwtSettings["ClockSkewMinutes"])),
        RoleClaimType = "role",
        NameClaimType = "sub"
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("Authentication failed: {Exception}", context.Exception.Message);

            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();

            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                var allClaims = identity.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
                logger.LogInformation("Token validated. Claims: {Claims}", string.Join("; ", allClaims));

                // Extract roles from the token
                var roleClaims = identity.FindAll("role").ToList();
                logger.LogInformation("Found {Count} role claims: {Roles}",
                    roleClaims.Count, string.Join(", ", roleClaims.Select(r => r.Value)));
            }

            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Authentication challenge for {Path}. Error: {Error}",
                context.Request.Path, context.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ===============================================
// 8. CORS CONFIGURATION
// ===============================================
var corsSettings = builder.Configuration.GetSection("CorsSettings");
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>();
        policy.WithOrigins(allowedOrigins ?? new[] { "*" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ===============================================
// 9. HEALTH CHECKS
// ===============================================
builder.Services.AddHealthChecks();

// ===============================================
// 10. HTTP CONTEXT ACCESSOR
// ===============================================
builder.Services.AddHttpContextAccessor();

// ===============================================
// 11. LOGGING
// ===============================================
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
    });
});

// ===============================================
// 12. OPENTELEMETRY CONFIGURATION
// ===============================================
var serviceName = builder.Configuration.GetValue<string>("ServiceName") ?? "AuthServer";
var serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        .AddAttributes(new Dictionary<string, object>
        {
            ["deployment.environment"] = builder.Environment.EnvironmentName,
            ["host.name"] = Environment.MachineName
        }))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = (httpContext) =>
            {
                // Don't trace health check endpoints
                return !httpContext.Request.Path.StartsWithSegments("/health");
            };
        })
        .AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
        })
        .AddSqlClientInstrumentation(options =>
        {
            options.RecordException = true;
        })
        .AddSource(serviceName)
        .AddConsoleExporter()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(
                builder.Configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint")
                ?? "http://localhost:4317");
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter(serviceName)
        .AddConsoleExporter()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(
                builder.Configuration.GetValue<string>("OpenTelemetry:OtlpEndpoint")
                ?? "http://localhost:4317");
        }));

// ===============================================
// BUILD THE APP
// ===============================================
var app = builder.Build();

// ===============================================
// MIDDLEWARE PIPELINE
// ===============================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth Server API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
    });
}
if (args.Contains("seed"))
{
    using var scope = app.Services.CreateScope();
    await SeedData(scope.ServiceProvider);
    return;
}

app.UseHttpsRedirection();
app.UseCors("DefaultCorsPolicy");
app.UseAuthentication();
app.UseMiddleware<AuthServer.Api.Middleware.AuthenticationLoggingMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

// ===============================================
// DATABASE SEEDING
// ===============================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AuthServerDbContext>();

        // Apply migrations automatically in development
        if (app.Environment.IsDevelopment())
        {
            context.Database.Migrate();
        }

        // Seed initial data
        await SeedData(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();

// ===============================================
// SEED DATA METHOD
// ===============================================
static async Task SeedData(IServiceProvider services)
{
    var unitOfWork = services.GetRequiredService<IUnitOfWork>();
    var configuration = services.GetRequiredService<IConfiguration>();

    // 1. Create default tenant
    var tenantExists = await unitOfWork.Tenants.ExistsAsync(t => true);
    Guid defaultTenantId;

    if (!tenantExists)
    {
        var defaultTenant = new AuthServer.Domain.Entities.Tenants.Tenant
        {
            Id = Guid.NewGuid(),
            Name = "Default Tenant",
            Subdomain = "default",
            Status = AuthServer.Domain.Enumerations.Enums.TenantStatus.Active,
            SubscriptionPlan = "Free"
        };

        await unitOfWork.Tenants.AddAsync(defaultTenant);
        await unitOfWork.SaveChangesAsync();

        defaultTenantId = defaultTenant.Id;
        Console.WriteLine($"✓ Created default tenant: {defaultTenantId}");
        Console.WriteLine($"  Subdomain: {defaultTenant.Subdomain}");
    }
    else
    {
        var existingTenant = await unitOfWork.Tenants.FirstOrDefaultAsync(t => true);
        defaultTenantId = existingTenant.Id;
        Console.WriteLine($"✓ Default tenant already exists: {defaultTenantId}");
    }

    // 2. Create super admin
    var adminExists = await unitOfWork.TenantAdmins.ExistsAsync(a => true);

    if (!adminExists)
    {
        var adminSettings = configuration.GetSection("AdminSettings");
        var adminEmail = adminSettings["DefaultSuperAdminEmail"];
        var adminPassword = adminSettings["DefaultSuperAdminPassword"];

        var superAdmin = new AuthServer.Domain.Entities.Tenants.TenantAdmin
        {
            Id = Guid.NewGuid(),
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            FirstName = "Super",
            LastName = "Admin",
            Role = AuthServer.Domain.Enumerations.Enums.AdminRole.SuperAdmin,
            IsActive = true
        };

        await unitOfWork.TenantAdmins.AddAsync(superAdmin);
        await unitOfWork.SaveChangesAsync();

        Console.WriteLine($"✓ Created super admin: {adminEmail}");
        Console.WriteLine($"  Password: {adminPassword}");
    }
    else
    {
        Console.WriteLine("✓ Super admin already exists");
    }

    // 3. Create default application
    var appExists = await unitOfWork.Applications.ExistsAsync(a => a.Name == "Default Application");

    Guid defaultAppId;
    if (!appExists)
    {
        var clientId = "default-client";
        var clientSecret = "default-secret-change-in-production";

        var defaultApp = new AuthServer.Domain.Entities.Applications.Application
        {
            Id = Guid.NewGuid(),
            Name = "Default Application",
            Description = "Default OAuth2 application for testing",
            ClientId = clientId,
            ClientSecretHash = BCrypt.Net.BCrypt.HashPassword(clientSecret),
            ApplicationType = AuthServer.Domain.Enumerations.Enums.ApplicationType.Native,
            AllowedGrantTypes = "password,refresh_token",
            AllowedScopes = "openid,profile,email",
            RedirectUris = "[\"http://localhost:3000/callback\",\"http://localhost:5000/callback\"]",
            PostLogoutRedirectUris = "[\"http://localhost:3000\",\"http://localhost:5000\"]",
            AllowedCorsOrigins = "[\"http://localhost:3000\",\"http://localhost:5000\"]",
            GoogleEnabled = false,
            AppleEnabled = false,
            LinkedInEnabled = false,
            HasLegacyDatabase = false,
            IsActive = true,
            GoogleClientId = string.Empty,
            GoogleClientSecret = string.Empty,
            AppleClientId = string.Empty,
            AppleTeamId = string.Empty,
            AppleKeyId = string.Empty,
            ApplePrivateKey = string.Empty,
            LinkedInClientId = string.Empty,
            LinkedInClientSecret = string.Empty,
            LegacyDatabaseConnectionString = string.Empty,
            LegacyUserTableName = string.Empty,
            LegacyUserIdColumn = string.Empty,
            LegacyEmailColumn = string.Empty,
            LegacyPasswordColumn = string.Empty,
            LegacyAdditionalColumnsMapping = string.Empty
        };

        await unitOfWork.Applications.AddAsync(defaultApp);
        await unitOfWork.SaveChangesAsync();

        defaultAppId = defaultApp.Id;
        Console.WriteLine($"✓ Created default application: {defaultApp.Id}");
        Console.WriteLine($"  Client ID: {clientId}");
        Console.WriteLine($"  Client Secret: {clientSecret}");

        // Create TenantApplication mapping
        var tenantApp = new AuthServer.Domain.Entities.Tenants.TenantApplication
        {
            Id = Guid.NewGuid(),
            TenantId = defaultTenantId,
            ApplicationId = defaultAppId
        };
        await unitOfWork.TenantApplications.AddAsync(tenantApp);
        await unitOfWork.SaveChangesAsync();
        Console.WriteLine($"✓ Linked application to default tenant");
    }
    else
    {
        var existingApp = await unitOfWork.Applications.FirstOrDefaultAsync(a => a.Name == "Default Application");
        defaultAppId = existingApp.Id;
        Console.WriteLine("✓ Default application already exists");
    }

    // 4. Create system admin user (no tenant)
    var systemAdminExists = await unitOfWork.Users.ExistsAsync(u => u.IsSystemAdmin && u.TenantId == null);

    if (!systemAdminExists)
    {
        var systemAdminEmail = "sysadmin@authserver.com";
        var systemAdminPassword = "SysAdmin@123456";

        var systemAdmin = new AuthServer.Domain.Entities.Users.User
        {
            Id = Guid.NewGuid(),
            TenantId = null,
            Email = systemAdminEmail,
            EmailVerified = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(systemAdminPassword),
            FirstName = "System",
            LastName = "Admin",
            IsSystemAdmin = true,
            Roles = "SuperAdmin,Admin",
            IsActive = true,
            LockoutEnabled = true,
            TwoFactorEnabled = false,
            PhoneNumber = string.Empty,
            LastLoginIp = string.Empty
        };

        await unitOfWork.Users.AddAsync(systemAdmin);
        await unitOfWork.SaveChangesAsync();

        Console.WriteLine($"✓ Created system admin user: {systemAdminEmail}");
        Console.WriteLine($"  Password: {systemAdminPassword}");
    }
    else
    {
        Console.WriteLine("✓ System admin user already exists");
    }

    // 5. Create default tenant user
    var userExists = await unitOfWork.Users.ExistsAsync(u => u.TenantId == defaultTenantId);

    if (!userExists)
    {
        var userEmail = "admin@example.com";
        var userPassword = "Admin@123456";

        var defaultUser = new AuthServer.Domain.Entities.Users.User
        {
            Id = Guid.NewGuid(),
            TenantId = defaultTenantId,
            Email = userEmail,
            EmailVerified = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userPassword),
            FirstName = "Tenant",
            LastName = "Admin",
            IsSystemAdmin = false,
            Roles = "TenantAdmin,Admin",
            IsActive = true,
            LockoutEnabled = true,
            TwoFactorEnabled = false,
            PhoneNumber = string.Empty,
            LastLoginIp = string.Empty
        };

        await unitOfWork.Users.AddAsync(defaultUser);
        await unitOfWork.SaveChangesAsync();

        Console.WriteLine($"✓ Created default tenant user: {userEmail}");
        Console.WriteLine($"  Password: {userPassword}");
    }
    else
    {
        Console.WriteLine("✓ Default tenant user already exists");
    }

    Console.WriteLine("\n==============================================");
    Console.WriteLine("SEED COMPLETED SUCCESSFULLY!");
    Console.WriteLine("==============================================");
    Console.WriteLine("IMPORTANT: Change all default passwords in production!");
    Console.WriteLine("==============================================\n");
}