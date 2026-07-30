using System.Threading.RateLimiting;
using HalcyonRecords.SeedDataGenerator.Core.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HalcyonRecords.SeedDataGenerator.Core.Wikipedia;

public static class WikipediaServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWikipediaClient(IConfiguration configuration)
        {
            var wikipediaOptions =
                configuration.GetSection(WikipediaOptions.SectionName).Get<WikipediaOptions>()
                ?? new WikipediaOptions();

            if (string.IsNullOrWhiteSpace(wikipediaOptions.BaseAddress))
            {
                throw new InvalidOperationException("Wikipedia:BaseAddress is not configured.");
            }

            if (string.IsNullOrWhiteSpace(wikipediaOptions.ContactEmail))
            {
                throw new InvalidOperationException(
                    "Wikipedia:ContactEmail is not configured — set it via "
                        + "'dotnet user-secrets set \"Wikipedia:ContactEmail\" \"...\"'."
                );
            }

            services
                .AddHttpClient<WikipediaClient>(client =>
                {
                    client.BaseAddress = new Uri(wikipediaOptions.BaseAddress);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        SeedToolUserAgent.For(wikipediaOptions.ContactEmail)
                    );
                })
                .AddStandardResilienceHandler(options =>
                {
                    var throughputLimiter = new SlidingWindowRateLimiter(
                        wikipediaOptions.RateLimit
                    );

                    options.RateLimiter.RateLimiter = args =>
                        throughputLimiter.AcquireAsync(1, args.Context.CancellationToken);
                });

            return services;
        }
    }
}
