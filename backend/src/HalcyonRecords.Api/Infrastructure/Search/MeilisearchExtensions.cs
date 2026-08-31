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

            services.AddHttpClient(HttpClientName);

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
