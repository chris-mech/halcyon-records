using HalcyonRecords.SeedDataGenerator.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HalcyonRecords.SeedDataGenerator.Core.Session;

public static class SeedDataSessionServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSeedDataSession(IConfiguration configuration)
        {
            var options =
                configuration.GetSection("SeedDataSession").Get<SeedDataSessionOptions>() ?? new();

            if (string.IsNullOrWhiteSpace(options.SeedDataFolder))
            {
                throw new InvalidOperationException(
                    "SeedDataSession:SeedDataFolder is not configured."
                );
            }

            services.AddSingleton<IReleaseService, ReleaseService>();
            services.AddSingleton<IReleaseGroupService, ReleaseGroupService>();
            services.AddSingleton<IMusicBrainzArtistService, MusicBrainzArtistService>();
            services.AddSingleton<IDiscogsArtistService, DiscogsArtistService>();
            services.AddSingleton<IGenreService, GenreService>();
            services.AddSingleton<ICoverImageService, CoverImageService>();
            services.AddSingleton<IDescriptionService, DescriptionService>();
            services.AddSingleton<IDiscogsMasterSearchService, DiscogsMasterSearchService>();
            services.AddSingleton<
                IMusicBrainzArtistSearchService,
                MusicBrainzArtistSearchService
            >();
            services.AddSingleton<
                IMusicBrainzReleaseSearchService,
                MusicBrainzReleaseSearchService
            >();

            services.AddSingleton<SeedDataSession>(sp =>
                ActivatorUtilities.CreateInstance<SeedDataSession>(sp, options)
            );

            return services;
        }
    }
}
