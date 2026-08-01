using Microsoft.Extensions.DependencyInjection;

namespace HalcyonRecords.SeedDataGenerator.Core.Enrichment;

public static class EnrichmentServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddEnrichment()
        {
            services.AddSingleton<WikipediaDescriptionResolver>();
            services.AddSingleton<IArtistEnricher, ArtistEnricher>();
            services.AddSingleton<IAlbumEnricher, AlbumEnricher>();

            return services;
        }
    }
}
