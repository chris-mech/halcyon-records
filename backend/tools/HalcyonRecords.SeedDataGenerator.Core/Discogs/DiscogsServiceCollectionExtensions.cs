using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HalcyonRecords.SeedDataGenerator.Core.Discogs;

public static class DiscogsServiceCollectionExtensions
{
    private static readonly string AppVersion = Assembly.GetExecutingAssembly().GetName().Version
        is { } version
        ? $"{version.Major}.{version.Minor}"
        : "0.0";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddDiscogsClient(IConfiguration configuration)
        {
            var discogsOptions =
                configuration.GetSection(DiscogsOptions.SectionName).Get<DiscogsOptions>()
                ?? new DiscogsOptions();

            if (string.IsNullOrWhiteSpace(discogsOptions.BaseAddress))
            {
                throw new InvalidOperationException("Discogs:BaseAddress is not configured.");
            }

            if (
                string.IsNullOrWhiteSpace(discogsOptions.ConsumerKey)
                || string.IsNullOrWhiteSpace(discogsOptions.ConsumerSecret)
            )
            {
                throw new InvalidOperationException(
                    "Discogs:ConsumerKey / Discogs:ConsumerSecret are not configured — set them via "
                        + "'dotnet user-secrets set \"Discogs:ConsumerKey\" \"...\"'."
                );
            }

            if (string.IsNullOrWhiteSpace(discogsOptions.ContactEmail))
            {
                throw new InvalidOperationException("Discogs:ContactEmail is not configured.");
            }

            services
                .AddHttpClient<DiscogsClient>(client =>
                {
                    client.BaseAddress = new Uri(discogsOptions.BaseAddress);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        $"HalcyonRecordsSeedTool/{AppVersion} ({discogsOptions.ContactEmail})"
                    );
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Discogs",
                        $"key={discogsOptions.ConsumerKey}, secret={discogsOptions.ConsumerSecret}"
                    );
                })
                .AddStandardResilienceHandler(options =>
                {
                    var throughputLimiter = new SlidingWindowRateLimiter(
                        new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 60,
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
