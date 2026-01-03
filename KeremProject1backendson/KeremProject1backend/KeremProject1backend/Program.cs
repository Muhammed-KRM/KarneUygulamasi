using KeremProject1backend.Services;
using Microsoft.AspNetCore.Http;
using FluentValidation.AspNetCore;
using FluentValidation;
using Hangfire;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
// using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Servisleri ekle
builder.Services.AddAppServices(builder.Configuration);

// HttpClient Fabrikası
builder.Services.AddHttpClient();

// Controller ve Swagger
// Controller ve Swagger
builder.Services.AddControllers();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Register Validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddEndpointsApiExplorer();

// SignalR
builder.Services.AddSignalR();

// File Service
builder.Services.AddScoped<KeremProject1backend.Core.Interfaces.IFileService, KeremProject1backend.Infrastructure.Services.LocalFileService>();

// 1. Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 2,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// 2. Caching (Redis)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "KarneProject_";
});
builder.Services.AddScoped<KeremProject1backend.Core.Interfaces.ICacheService, KeremProject1backend.Infrastructure.Services.RedisCacheService>();

// 3. Background Jobs (Hangfire)
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(Hangfire.CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();
// builder.Services.AddSwaggerGen(c =>
// {
//     // Döngüsel referansları önlemek için
//     c.UseAllOfToExtendReferenceSchemas();
//     c.UseOneOfForPolymorphism();
//     c.UseAllOfForInheritance();
// 
//     // Hata yönetimi - benzersiz schema ID'leri
//     c.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
// 
//     // IgnoreObsoleteActions - kullanılmayan action'ları yok say
//     c.IgnoreObsoleteActions();
// 
//     // IgnoreObsoleteProperties - kullanılmayan property'leri yok say
//     c.IgnoreObsoleteProperties();
// });

var app = builder.Build();

// HTTP request pipeline
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

// CORS politikasını kullan
app.UseCors("AllowAngularApp");

// Middleware Kaydı
app.UseRateLimiter(); // Rate Limiting Middleware
app.UseMiddleware<KeremProject1backend.Middlewares.RequestLoggingMiddleware>();
app.UseMiddleware<KeremProject1backend.Middlewares.GlobalExceptionMiddleware>();

app.UseHangfireDashboard(); // Hangfire Dashboard

app.MapControllers();
app.MapHub<KeremProject1backend.Hubs.NotificationHub>("/notificationHub");
app.MapHub<KeremProject1backend.Hubs.ChatHub>("/chatHub");

app.Run();
