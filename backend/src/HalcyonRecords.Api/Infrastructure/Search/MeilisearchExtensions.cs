using Meilisearch;

namespace HalcyonRecords.Api.Infrastructure.Search;

public static class MeilisearchExtensions
{
    private const string HttpClientName = "meilisearch";

    extension(IServiceCollection services)
    {
        public IServiceCollection AddApiMeilisearch(IConfiguration configuration)
        {
            services.Configure<MeilisearchIndexOptions>(
                configuration.GetSection(MeilisearchIndexOptions.SectionName)
            );

            services
                .AddHttpClient(HttpClientName)
                .UseSocketsHttpHandler(
                    (handler, _) => handler.PooledConnectionLifetime = TimeSpan.FromMinutes(2)
                )
                .SetHandlerLifetime(Timeout.InfiniteTimeSpan)
                .AddStandardResilienceHandler(options =>
                {
                    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(20);
                    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(60);
                    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(40);
                });

            services.AddSingleton(sp =>
            {
                var connectionInfo = MeilisearchConnectionInfo.Parse(
                    configuration.GetConnectionString("meilisearch"),
                    configuration["Meilisearch:MasterKey"]
                );

                var httpClient = sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(HttpClientName);
                httpClient.BaseAddress = connectionInfo.Endpoint;

                return new MeilisearchClient(httpClient, connectionInfo.MasterKey);
            });

            services.AddHealthChecks().AddCheck<MeilisearchHealthCheck>("meilisearch");

            services.AddSingleton<MeilisearchIndexer>();

            return services;
        }
    }
}
