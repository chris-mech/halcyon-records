using System.Threading.RateLimiting;
using HalcyonRecords.SeedDataGenerator.Core.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;

public static class MusicBrainzServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMusicBrainzClient(IConfiguration configuration)
        {
            var musicBrainzOptions =
                configuration.GetSection(MusicBrainzOptions.SectionName).Get<MusicBrainzOptions>()
                ?? new MusicBrainzOptions();

            if (string.IsNullOrWhiteSpace(musicBrainzOptions.BaseAddress))
            {
                throw new InvalidOperationException("MusicBrainz:BaseAddress is not configured.");
            }

            if (string.IsNullOrWhiteSpace(musicBrainzOptions.ContactEmail))
            {
                throw new InvalidOperationException(
                    "MusicBrainz:ContactEmail is not configured — set it via "
                        + "'dotnet user-secrets set \"MusicBrainz:ContactEmail\" \"...\"'."
                );
            }

            services
                .AddHttpClient<MusicBrainzClient>(client =>
                {
                    client.BaseAddress = new Uri(musicBrainzOptions.BaseAddress);
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(
                        SeedToolUserAgent.For(musicBrainzOptions.ContactEmail)
                    );
                })
                .AddStandardResilienceHandler(options =>
                {
                    var throughputLimiter = new TokenBucketRateLimiter(
                        musicBrainzOptions.RateLimit
                    );

                    options.RateLimiter.RateLimiter = args =>
                        throughputLimiter.AcquireAsync(1, args.Context.CancellationToken);
                });

            return services;
        }
    }
}
