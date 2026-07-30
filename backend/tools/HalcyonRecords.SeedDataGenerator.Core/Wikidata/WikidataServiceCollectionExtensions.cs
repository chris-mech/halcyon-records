using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HalcyonRecords.SeedDataGenerator.Core.Wikidata;

public static class WikidataServiceCollectionExtensions
{
    private static readonly string AppVersion = Assembly.GetExecutingAssembly().GetName().Version
        is { } version
        ? $"{version.Major}.{version.Minor}"
        : "0.0";

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
                    "Wikidata:ContactEmail is not configured — set it via "
                        + "'dotnet user-secrets set \"Wikidata:ContactEmail\" \"...\"'."
                );
            }

            services
                .AddHttpClient<WikidataClient>(client =>
                {
                    client.BaseAddress = new Uri(wikidataOptions.BaseAddress);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        $"HalcyonRecordsSeedTool/{AppVersion} ({wikidataOptions.ContactEmail})"
                    );
                })
                .AddStandardResilienceHandler(options =>
                {
                    var throughputLimiter = new SlidingWindowRateLimiter(
                        new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 200,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6,
                            AutoReplenishment = true,
                            QueueLimit = int.MaxValue,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        }
                    );

                    options.RateLimiter.RateLimiter = args =>
                        throughputLimiter.AcquireAsync(1, args.Context.CancellationToken);
                });

            return services;
        }
    }
}
