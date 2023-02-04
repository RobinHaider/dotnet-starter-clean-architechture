using AspNetCoreRateLimit;
using Microsoft.Extensions.Configuration;

namespace API.Extensions
{
    public static class RateLimitingServiceExtensionOld
    {
        public static IServiceCollection AddAndConfigureRateLimitingOld(this IServiceCollection services, IConfiguration configuration)
        {
            

            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();

            //load general configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));

            //load ip rules from appsettings.json
            services.Configure<IpRateLimitPolicies>(configuration.GetSection("IpRateLimitPolicies"));

            // inject counter and rules stores
            services.AddInMemoryRateLimiting();
            //services.AddDistributedRateLimiting<AsyncKeyLockProcessingStrategy>();
            //services.AddDistributedRateLimiting<RedisProcessingStrategy>();
            //services.AddRedisRateLimiting();

            // configuration (resolvers, counter key builders)
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            return services;
        }
    }
}
