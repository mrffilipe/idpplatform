using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using IdPPlatform.API.Middlewares;
using IdPPlatform.Domain.Constants;
using IdPPlatform.Infrastructure.Configurations;
using IdPPlatform.Infrastructure.Extensions;
using IdPPlatform.Infrastructure.Persistence;
using TenancyKit.AspNetCore;
using TenancyKit.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    });

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPlatformOidc(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlatformAdministrator", policy =>
        policy.RequireClaim(PlatformRoleDefaults.ClaimType, PlatformRoleDefaults.PlatformAdministrator));
});

builder.Services.AddRateLimiter(options =>
{
    var rateLimitOptions = builder.Configuration.GetSection(RateLimitOptions.Section).Get<RateLimitOptions>()
        ?? new RateLimitOptions();

    options.AddPolicy("platform_bootstrap", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitOptions.BootstrapPermitLimit,
                Window = TimeSpan.FromMinutes(rateLimitOptions.BootstrapWindowMinutes),
                QueueLimit = 0
            }));
});

builder.Services
    .AddTenancyKit<TenantInfoAdapter>(options =>
    {
        options.UseMissingTenantBehavior(MissingTenantBehavior.Ignore);
        options.UseClaimsTenantResolver();
        options.UseStore<TenantStore>();
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseMiddleware<ApplicationExceptionMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseMultiTenancy<TenantInfoAdapter>();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
