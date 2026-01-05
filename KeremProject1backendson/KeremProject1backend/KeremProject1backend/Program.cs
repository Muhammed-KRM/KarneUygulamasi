using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using KeremProject1backend.Infrastructure;
using KeremProject1backend.Services;
using KeremProject1backend.Operations;
using KeremProject1backend.Middlewares;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

// 1. Database Context
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Redis Cache
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "KarneProject_";
});

// Redis Connection Multiplexer for advanced operations (pattern matching, etc.)
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
        ConnectionMultiplexer.Connect(redisConnectionString));
}

// 3. Core Services
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<CacheService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<OpticalParserService>();
builder.Services.AddScoped<InstitutionOperations>();
builder.Services.AddScoped<ClassroomOperations>();
builder.Services.AddScoped<ExamOperations>();
builder.Services.AddScoped<MessageOperations>();
builder.Services.AddScoped<AccountOperations>();
builder.Services.AddScoped<UserOperations>();
builder.Services.AddScoped<AdminOperations>();

// Background Jobs
builder.Services.AddScoped<KeremProject1backend.Jobs.CalculateRankingsJob>();
builder.Services.AddScoped<KeremProject1backend.Jobs.BulkNotificationJob>();

// 4. Authentication (JWT)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!))
    };

    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = KeremProject1backend.Models.DTOs.BaseResponse<string>.ErrorResponse(
                "Unauthorized: Session invalid or missing",
                KeremProject1backend.Core.Constants.ErrorCodes.Unauthorized);

            return context.Response.WriteAsJsonAsync(response);
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";

            var response = KeremProject1backend.Models.DTOs.BaseResponse<string>.ErrorResponse(
                "Forbidden: Insufficient permissions",
                KeremProject1backend.Core.Constants.ErrorCodes.AccessDenied);

            return context.Response.WriteAsJsonAsync(response);
        }
    };
});

// 4.1. Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Institution Role Policies (can be used with [Authorize(Policy = "InstitutionRole:Manager,Teacher")])
    options.AddPolicy("InstitutionRole:Manager", policy => policy.RequireAssertion(context =>
    {
        // This will be checked in controllers using SessionService
        return true; // Actual check done in controller/operation layer
    }));
    
    options.AddPolicy("InstitutionRole:Teacher", policy => policy.RequireAssertion(context => true));
    options.AddPolicy("InstitutionRole:Student", policy => policy.RequireAssertion(context => true));
});

// 4.2. Rate Limiting (Very broad limits for high traffic)
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 1000, // 1000 requests
                Window = TimeSpan.FromMinutes(1) // per minute (very broad)
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(
            KeremProject1backend.Models.DTOs.BaseResponse<string>.ErrorResponse(
                "Too many requests. Please try again later.",
                "100429"), cancellationToken);
    };
});

// 5. Background Jobs (Hangfire)
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// 6. Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KarneProject API", Version = "v1" });

    // JWT Support in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
            new string[] {}
        }
    });
});

// 7. Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// 8. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// 9. SignalR
builder.Services.AddSignalR();


var app = builder.Build();

// Configure Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Middleware Pipeline (Order matters!)
app.UseMiddleware<RequestLoggingMiddleware>(); // Log requests first
app.UseMiddleware<TokenBlacklistMiddleware>(); // Check token blacklist
app.UseMiddleware<GlobalExceptionMiddleware>(); // Catch exceptions

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter(); // Rate limiting middleware

app.UseHangfireDashboard();

app.MapControllers();

// SignalR Hubs
app.MapHub<KeremProject1backend.Hubs.ChatHub>("/hubs/chat");
app.MapHub<KeremProject1backend.Hubs.NotificationHub>("/hubs/notification");

// Seed Data
await KeremProject1backend.Infrastructure.Data.DataSeeder.SeedAsync(app);

app.Run();
