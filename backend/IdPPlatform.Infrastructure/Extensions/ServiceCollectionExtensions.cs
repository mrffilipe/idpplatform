using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmailV2;
using IdPPlatform.Application.Services.IdentityProvider;
using IdPPlatform.Application.Services.Security;
using IdPPlatform.Infrastructure.Configurations;
using IdPPlatform.Infrastructure.Persistence;
using IdPPlatform.Infrastructure.Persistence.Interceptors;
using IdPPlatform.Infrastructure.Services.Security;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IdPPlatform.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.Section))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.Section))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();

        services.AddOptions<RateLimitOptions>()
            .Bind(configuration.GetSection(RateLimitOptions.Section))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<RateLimitOptions>, RateLimitOptionsValidator>();

        services.AddOptions<InviteOptions>()
            .Bind(configuration.GetSection(InviteOptions.Section))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<InviteOptions>, InviteOptionsValidator>();

        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.Section))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<EmailOptions>, EmailOptionsValidator>();

        services.AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.Section))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<RedisOptions>, RedisOptionsValidator>();

        services.AddOptions<BootstrapOptions>()
            .Bind(configuration.GetSection(BootstrapOptions.Section))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<BootstrapOptions>, BootstrapOptionsValidator>();

        services.AddOptions<SecretProtectionOptions>()
            .Bind(configuration.GetSection(SecretProtectionOptions.Section))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<SecretProtectionOptions>, SecretProtectionOptionsValidator>();

        services.AddOptions<PasswordPolicyOptions>()
            .Bind(configuration.GetSection(PasswordPolicyOptions.Section))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<PasswordPolicyOptions>, PasswordPolicyOptionsValidator>();

        services.AddSecretProtection(configuration);

        services.AddHttpContextAccessor();
        services.AddDistributedCaching(configuration);
        services.AddSingleton<IAmazonSimpleEmailServiceV2>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<EmailOptions>>().Value;
            var region = RegionEndpoint.GetBySystemName(options.Region);

            if (!string.IsNullOrWhiteSpace(options.AccessKeyId) &&
                !string.IsNullOrWhiteSpace(options.SecretAccessKey))
            {
                if (!string.IsNullOrWhiteSpace(options.SessionToken))
                {
                    return new AmazonSimpleEmailServiceV2Client(
                        new SessionAWSCredentials(
                            options.AccessKeyId,
                            options.SecretAccessKey,
                            options.SessionToken),
                        region);
                }

                return new AmazonSimpleEmailServiceV2Client(
                    new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey),
                    region);
            }

            return new AmazonSimpleEmailServiceV2Client(region);
        });

        services.AddScoped<TenantStore>();
        services.AddDbContext(configuration);
        services.AddRepositories();
        services.AddAggregateServices();
        services.AddServices();
        services.AddIdentityProviderFederation();

        return services;
    }

    private static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetSection(DatabaseOptions.Section)["ConnectionString"];
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditInterceptor>());
        });
        return services;
    }

    private static IServiceCollection AddDistributedCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var redisOptions = configuration.GetSection(RedisOptions.Section).Get<RedisOptions>() ?? new RedisOptions();
        if (!string.IsNullOrWhiteSpace(redisOptions.ConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions.ConnectionString;
                options.InstanceName = redisOptions.InstanceName;
            });
            return services;
        }

        services.AddDistributedMemoryCache();
        return services;
    }

    private static IServiceCollection AddSecretProtection(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(SecretProtectionOptions.Section).Get<SecretProtectionOptions>()
                      ?? new SecretProtectionOptions();

        var keyDirectory = Path.IsPathRooted(options.KeyDirectoryPath)
            ? options.KeyDirectoryPath
            : Path.Combine(AppContext.BaseDirectory, options.KeyDirectoryPath);
        Directory.CreateDirectory(keyDirectory);

        services.AddDataProtection()
            .SetApplicationName(options.ApplicationName)
            .PersistKeysToFileSystem(new DirectoryInfo(keyDirectory));

        services.AddSingleton<ISecretProtector, DataProtectionSecretProtector>();
        services.AddSingleton<IIdentityProviderConfigCipher, IdentityProviderConfigCipher>();

        return services;
    }
}
