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

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(int.Parse(jwtSettings["ClockSkewMinutes"]))
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
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

    // Check if we have any tenants
    var tenantExists = await unitOfWork.Tenants.ExistsAsync(t => true);

    if (!tenantExists)
    {
        // Create default tenant
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

        Console.WriteLine($"Created default tenant: {defaultTenant.Id}");
        Console.WriteLine($"Subdomain: {defaultTenant.Subdomain}");
    }

    // Check if we have any admin users
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

        Console.WriteLine($"Created super admin: {adminEmail}");
        Console.WriteLine($"Password: {adminPassword}");
        Console.WriteLine("IMPORTANT: Change this password in production!");
    }
}