using System.Threading.RateLimiting;
using HalcyonRecords.SeedDataGenerator.Core.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HalcyonRecords.SeedDataGenerator.Core.CoverArtArchive;

public static class CoverArtArchiveServiceCollectionExtensions
{
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
                        SeedToolUserAgent.For(coverArtArchiveOptions.ContactEmail)
                    );
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new HttpClientHandler { AllowAutoRedirect = false }
                )
                .AddStandardResilienceHandler(options =>
                {
                    var throughputLimiter = new TokenBucketRateLimiter(
                        coverArtArchiveOptions.RateLimit
                    );

                    options.RateLimiter.RateLimiter = args =>
                        throughputLimiter.AcquireAsync(1, args.Context.CancellationToken);
                });

            return services;
        }
    }
}
