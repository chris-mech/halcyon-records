using System.Net.Http.Headers;
using System.Threading.RateLimiting;
using HalcyonRecords.SeedDataGenerator.Core.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HalcyonRecords.SeedDataGenerator.Core.Discogs;

public static class DiscogsServiceCollectionExtensions
{
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
                    "Discogs:ConsumerKey / Discogs:ConsumerSecret are not configured: set them via "
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
                        SeedToolUserAgent.For(discogsOptions.ContactEmail)
                    );
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Discogs",
                        $"key={discogsOptions.ConsumerKey}, secret={discogsOptions.ConsumerSecret}"
                    );
                })
                .AddStandardResilienceHandler(options =>
                {
                    var throughputLimiter = new SlidingWindowRateLimiter(discogsOptions.RateLimit);

                    options.RateLimiter.RateLimiter = args =>
                        throughputLimiter.AcquireAsync(1, args.Context.CancellationToken);
                });

            return services;
        }
    }
}
