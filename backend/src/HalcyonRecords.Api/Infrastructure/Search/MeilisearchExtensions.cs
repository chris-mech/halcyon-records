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
                var clientOptions = MeilisearchClientOptions.Parse(
                    configuration.GetConnectionString("meilisearch")
                );

                var httpClient = sp.GetRequiredService<IHttpClientFactory>()
                    .CreateClient(HttpClientName);
                httpClient.BaseAddress = clientOptions.Endpoint;

                return new MeilisearchClient(httpClient, clientOptions.MasterKey);
            });

            services.AddHealthChecks().AddCheck<MeilisearchHealthCheck>("meilisearch");

            services.AddSingleton<MeilisearchIndexer>();

            return services;
        }
    }
}
