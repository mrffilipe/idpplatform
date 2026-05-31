using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Kyvo.API.Components;
using Kyvo.API.Middlewares;
using Kyvo.API.Services;
using Kyvo.API.Swagger;
using Kyvo.Domain.Constants;
using Kyvo.Infrastructure.Configurations;
using Kyvo.Infrastructure.Extensions;
using Kyvo.Infrastructure.Persistence;
using Microsoft.AspNetCore.HttpOverrides;
using TenancyKit.AspNetCore;
using TenancyKit.Core;

var builder = WebApplication.CreateBuilder(args);

// --- MVC / JSON API ---
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddApiDocumentation();

// --- Blazor account UI (SSR) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorComponents();
builder.Services.AddSingleton<IFederatedConfigBuilder, FederatedConfigBuilder>();

// --- API versioning ---
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    });

// --- Domain infrastructure (EF, services, auth) ---
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPlatformOidc(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlatformAdministrator", policy =>
        policy.RequireClaim(PlatformRoleDefaults.ClaimType, PlatformRoleDefaults.PlatformAdministrator));
});

// --- Rate limiting ---
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

    options.AddPolicy("account_register", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = rateLimitOptions.AccountRegisterPermitLimit,
                Window = TimeSpan.FromMinutes(rateLimitOptions.AccountRegisterWindowMinutes),
                QueueLimit = 0
            }));
});

// --- Multi-tenancy ---
builder.Services
    .AddTenancyKit<TenantInfoAdapter>(options =>
    {
        options.UseMissingTenantBehavior(MissingTenantBehavior.Ignore);
        options.UseClaimsTenantResolver();
        options.UseStore<TenantStore>();
    });

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy => policy
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());
});

// --- Reverse proxy / forwarded headers ---
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// --- Antiforgery (Blazor forms + account POST) ---
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// --- Middleware pipeline ---
app.UseForwardedHeaders();
app.UseMiddleware<ApplicationExceptionMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "REST API v1");
        options.SwaggerEndpoint("/swagger/oidc/swagger.json", "OAuth / OIDC / Account");
    });
}

app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();
app.UseMultiTenancy<TenantInfoAdapter>();
app.UseAuthorization();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();
app.MapRazorComponents<App>();

app.Run();
