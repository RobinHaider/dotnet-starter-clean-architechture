using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace API.Extensions
{
    public static class RateLimitingServiceExtension
    {
        public static IServiceCollection AddAndConfigureRateLimiting(this IServiceCollection services)
        {

            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("Fixed", options =>
                {                  
                    options.Window = TimeSpan.FromSeconds(30);
                    options.PermitLimit = 1;
                    options.AutoReplenishment = true;
                });

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
