using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HalcyonRecords.SeedDataGenerator.Core.Wikipedia;

public static class WikipediaServiceCollectionExtensions
{
    private static readonly string AppVersion = Assembly.GetExecutingAssembly().GetName().Version
        is { } version
        ? $"{version.Major}.{version.Minor}"
        : "0.0";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddWikidataClient(IConfiguration configuration)
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
                        $"HalcyonRecordsSeedTool/{AppVersion} ({wikipediaOptions.ContactEmail})"
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
