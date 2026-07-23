using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace HalcyonRecords.Api.Common.RateLimiting;

public static class RateLimitingExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApiRateLimiting(IConfiguration configuration)
        {
            var rateLimitOptions =
                configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>()
                ?? new RateLimitOptions();

            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                    httpContext =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()
                                ?? "unknown",
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                AutoReplenishment = true,
                                PermitLimit = rateLimitOptions.PermitLimit,
                                Window = TimeSpan.FromSeconds(rateLimitOptions.WindowSeconds),
                                QueueLimit = 0,
                            }
                        )
                );

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter = (
                            (int)retryAfter.TotalSeconds
                        ).ToString(NumberFormatInfo.InvariantInfo);
                    }

                    var problemDetailsService =
                        context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

                    await problemDetailsService.TryWriteAsync(
                        new ProblemDetailsContext
                        {
                            HttpContext = context.HttpContext,
                            ProblemDetails = new ProblemDetails
                            {
                                Status = StatusCodes.Status429TooManyRequests,
                                Title = "Too Many Requests",
                                Detail = "Rate limit exceeded. Please try again later.",
                            },
                        }
                    );
                };
            });

            return services;
        }
    }
}
