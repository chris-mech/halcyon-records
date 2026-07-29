using System.Reflection;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HalcyonRecords.SeedDataGenerator.Core.CoverArtArchive;

public static class CoverArtArchiveServiceCollectionExtensions
{
    private static readonly string AppVersion = Assembly.GetExecutingAssembly().GetName().Version
        is { } version
        ? $"{version.Major}.{version.Minor}"
        : "0.0";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddCoverArtArchiveClient(IConfiguration configuration)
        {
            var coverArtArchiveOptions =
                configuration
                    .GetSection(CoverArtArchiveOptions.SectionName)
                    .Get<CoverArtArchiveOptions>()
                ?? new CoverArtArchiveOptions();

            if (string.IsNullOrWhiteSpace(coverArtArchiveOptions.BaseAddress))
            {
                throw new InvalidOperationException(
                    "CoverArtArchive:BaseAddress is not configured."
                );
            }

            if (string.IsNullOrWhiteSpace(coverArtArchiveOptions.ContactEmail))
            {
                throw new InvalidOperationException(
                    "CoverArtArchive:ContactEmail is not configured — set it via "
                        + "'dotnet user-secrets set \"CoverArtArchive:ContactEmail\" \"...\"'."
                );
            }

            services
                .AddHttpClient<CoverArtArchiveClient>(client =>
                {
                    client.BaseAddress = new Uri(coverArtArchiveOptions.BaseAddress);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        $"HalcyonRecordsSeedTool/{AppVersion} ({coverArtArchiveOptions.ContactEmail})"
                    );
                })
                .AddStandardResilienceHandler(options =>
                {
                    var throughputLimiter = new TokenBucketRateLimiter(
                        new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = 1,
                            TokensPerPeriod = 1,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
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
