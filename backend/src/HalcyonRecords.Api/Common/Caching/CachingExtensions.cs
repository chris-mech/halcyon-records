using Microsoft.Extensions.Caching.Hybrid;

namespace HalcyonRecords.Api.Common.Caching;

public static class CachingExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApiHybridCache(IConfiguration configuration)
        {
            var cachingOptions =
                configuration.GetSection(CachingOptions.SectionName).Get<CachingOptions>()
                ?? new CachingOptions();

            services.AddHybridCache(options =>
            {
                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromSeconds(cachingOptions.TtlSeconds),
                    LocalCacheExpiration = TimeSpan.FromSeconds(cachingOptions.TtlSeconds),
                };
            });

            return services;
        }
    }
}
