using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace API.Extensions
{
    public static class RateLimitingNetSevenServiceExtension
    {
        public static IServiceCollection AddAndConfigureRateLimitingNetSeven(this IServiceCollection services)
        {

            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("Fixed", options =>
                {                  
                    options.Window = TimeSpan.FromSeconds(30);
                    options.PermitLimit = 10;
                    options.AutoReplenishment = true;
                });

                // Let’s add a simple rate limiter that limits all to 10 requests per minute, per authenticated username (or hostname if not authenticated):
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 20,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // on rejection
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        await context.HttpContext.Response.WriteAsync(
                            $"Too many requests. Please try again after {retryAfter.TotalMinutes} minute(s). " +
                            $"Read more about our rate limits at https://example.org/docs/ratelimiting.", cancellationToken: token);
                    }
                    else
                    {
                        await context.HttpContext.Response.WriteAsync(
                            "Too many requests. Please try again later. " +
                            "Read more about our rate limits at https://example.org/docs/ratelimiting.", cancellationToken: token);
                    }
                };

                // ...
            });

            return services;
        }
    }
}
