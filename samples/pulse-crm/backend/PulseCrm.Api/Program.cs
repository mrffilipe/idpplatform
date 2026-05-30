using System.Text.Json.Serialization;
using IdPPlatform.AspNetCore;
using IdPPlatform.AspNetCore.TenancyKit;
using IdPPlatform.Client;
using Microsoft.EntityFrameworkCore;
using PulseCrm.Api.Configuration;
using PulseCrm.Api.Data;
using PulseCrm.Api.Services;
using TenancyKit.AspNetCore;
using TenancyKit.Core;
using TenancyKit.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.Section));

var allowInvalidIdpCertificate = builder.Configuration.GetValue(
    "IdP:AllowInvalidIdpCertificate",
    builder.Environment.IsDevelopment());

builder.Services
    .AddIdPPlatformAuthentication(builder.Configuration, IdPPlatformOptions.SectionName)
    .PostConfigure<IdPPlatformOptions>(o => o.AllowInvalidCertificate = allowInvalidIdpCertificate);

builder.Services
    .AddIdPPlatformClient(builder.Configuration)
    .PostConfigure<IdPPlatformClientOptions>(o => o.AllowInvalidCertificate = allowInvalidIdpCertificate);

builder.Services
    .AddIdPPlatformTenancyKit<ProductTenantInfo>(options =>
    {
        options.UseMissingTenantBehavior(MissingTenantBehavior.Ignore);
        options.UseClaimsTenantResolver("tid");
        options.UseClaimPassthroughTenantStore();
        options.ConfigureEntity<ITenantOwned, Guid>(e => e.TenantId);
    });

builder.Services.AddScoped<TenancyKitSaveChangesInterceptor<ProductTenantInfo>>();
builder.Services.AddDbContext<PulseCrmDbContext>((provider, options) =>
{
    options
        .UseSqlite(builder.Configuration.GetConnectionString("PulseCrm"))
        .AddInterceptors(provider.GetRequiredService<TenancyKitSaveChangesInterceptor<ProductTenantInfo>>());
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var corsOrigins = builder.Configuration.GetSection(CorsOptions.Section).Get<CorsOptions>()?.AllowedOrigins
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("PulseCrmSpa", policy =>
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PulseCrmDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PulseCrmSpa");
app.UseAuthentication();
app.UseMultiTenancy<ProductTenantInfo>();
app.UseAuthorization();
app.MapControllers();

app.Run();
