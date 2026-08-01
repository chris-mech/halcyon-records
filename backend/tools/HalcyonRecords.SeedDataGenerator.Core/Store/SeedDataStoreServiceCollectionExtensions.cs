using HalcyonRecords.SeedDataGenerator.Core.Enrichment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HalcyonRecords.SeedDataGenerator.Core.Store;

public static class SeedDataStoreServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSeedDataStore(IConfiguration configuration)
        {
            var options =
                configuration.GetSection("SeedDataStore").Get<SeedDataStoreOptions>() ?? new();

            if (string.IsNullOrWhiteSpace(options.SeedDataFolder))
            {
                throw new InvalidOperationException(
                    "SeedDataStore:SeedDataFolder is not configured."
                );
            }

            services.AddEnrichment();

            services.AddSingleton<SeedDataStore>(sp =>
                ActivatorUtilities.CreateInstance<SeedDataStore>(sp, options)
            );

            return services;
        }
    }
}
