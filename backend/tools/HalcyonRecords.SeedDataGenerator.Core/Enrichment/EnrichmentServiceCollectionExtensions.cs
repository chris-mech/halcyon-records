using HalcyonRecords.SeedDataGenerator.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HalcyonRecords.SeedDataGenerator.Core.Enrichment;

public static class EnrichmentServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddEnrichment()
        {
            services.AddSingleton<IReleaseService, ReleaseService>();
            services.AddSingleton<IReleaseGroupService, ReleaseGroupService>();
            services.AddSingleton<IMusicBrainzArtistService, MusicBrainzArtistService>();
            services.AddSingleton<IDiscogsArtistService, DiscogsArtistService>();
            services.AddSingleton<IGenreService, GenreService>();
            services.AddSingleton<ICoverImageService, CoverImageService>();
            services.AddSingleton<IDescriptionService, DescriptionService>();
            services.AddSingleton<IArtistEnricher, ArtistEnricher>();
            services.AddSingleton<IAlbumEnricher, AlbumEnricher>();

            return services;
        }
    }
}
