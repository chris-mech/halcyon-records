using System.Threading.RateLimiting;
using HalcyonRecords.SeedDataGenerator.Core.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HalcyonRecords.SeedDataGenerator.Core.Wikidata;

public static class WikidataServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWikidataClient(IConfiguration configuration)
        {
            var wikidataOptions =
                configuration.GetSection(WikidataOptions.SectionName).Get<WikidataOptions>()
                ?? new WikidataOptions();

            if (string.IsNullOrWhiteSpace(wikidataOptions.BaseAddress))
            {
                throw new InvalidOperationException("Wikidata:BaseAddress is not configured.");
            }

            if (string.IsNullOrWhiteSpace(wikidataOptions.ContactEmail))
            {
                throw new InvalidOperationException(
                    "Wikidata:ContactEmail is not configured: set it via "
                        + "'dotnet user-secrets set \"Wikidata:ContactEmail\" \"...\"'."
                );
            }

            services
                .AddHttpClient<WikidataClient>(client =>
                {
                    client.BaseAddress = new Uri(wikidataOptions.BaseAddress);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        SeedToolUserAgent.For(wikidataOptions.ContactEmail)
                    );
                })
                .AddStandardResilienceHandler(options =>
                {
                    var throughputLimiter = new SlidingWindowRateLimiter(wikidataOptions.RateLimit);

                    options.RateLimiter.RateLimiter = args =>
                        throughputLimiter.AcquireAsync(1, args.Context.CancellationToken);
                });

            return services;
        }
    }
}
