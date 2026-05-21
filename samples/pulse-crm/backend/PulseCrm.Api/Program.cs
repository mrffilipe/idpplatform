using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PulseCrm.Api.Configuration;
using PulseCrm.Api.Data;
using PulseCrm.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<IdPOptions>(builder.Configuration.GetSection(IdPOptions.Section));
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.Section));

var idpOptions = builder.Configuration.GetSection(IdPOptions.Section).Get<IdPOptions>()
    ?? new IdPOptions();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = idpOptions.Authority.TrimEnd('/');
        options.Audience = idpOptions.Audience;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = idpOptions.Authority.TrimEnd('/'),
            ValidateAudience = true,
            ValidAudience = idpOptions.Audience,
            ValidateLifetime = true,
            NameClaimType = "email",
            RoleClaimType = "trole"
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.AddHttpClient<IIdPSubscribeClient, IdPSubscribeClient>(client =>
{
    client.BaseAddress = new Uri(idpOptions.Authority.TrimEnd('/') + "/");
});

builder.Services.AddDbContext<PulseCrmDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PulseCrm")));

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
app.UseAuthorization();
app.MapControllers();

app.Run();
