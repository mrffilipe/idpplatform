using Microsoft.Extensions.Options;

namespace Kyvo.Infrastructure.Configurations;

public sealed class RedisOptionsValidator : IValidateOptions<RedisOptions>
{
    public ValidateOptionsResult Validate(string? name, RedisOptions options)
    {
        var errors = new List<string>();

        // When a Redis connection string is provided, InstanceName must also be set
        // because Microsoft.Extensions.Caching.StackExchangeRedis uses it as a key prefix.
        if (!string.IsNullOrWhiteSpace(options.ConnectionString) &&
            string.IsNullOrWhiteSpace(options.InstanceName))
        {
            errors.Add("Redis:InstanceName is required when Redis:ConnectionString is set.");
        }

        if (options.TenantIdentifierCacheMinutes <= 0)
        {
            errors.Add("Redis:TenantIdentifierCacheMinutes must be greater than zero.");
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
