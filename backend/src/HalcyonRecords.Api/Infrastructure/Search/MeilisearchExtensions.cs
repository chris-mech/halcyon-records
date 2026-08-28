using System.Security.Cryptography;
using System.Text;
using Meilisearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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

    extension(WebApplication app)
    {
        public WebApplication MapMeilisearchMaintenanceEndpoints()
        {
            app.MapPost(
                    "/api/maintenance/search/reindex",
                    async (
                        [FromHeader(Name = "X-Reindex-Key")] string? triggerKey,
                        MeilisearchIndexer indexer,
                        ApplicationDbContext dbContext,
                        IOptions<ReindexOptions> options,
                        CancellationToken ct
                    ) =>
                    {
                        if (!TriggerKeyMatches(triggerKey, options.Value.TriggerKey))
                        {
                            return Results.Unauthorized();
                        }

                        await indexer.RebuildAsync(dbContext, ct);
                        return Results.NoContent();
                    }
                )
                .ExcludeFromDescription();

            return app;
        }
    }

    private static bool TriggerKeyMatches(string? provided, string? configured)
    {
        if (string.IsNullOrEmpty(provided) || string.IsNullOrEmpty(configured))
        {
            return false;
        }

        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var configuredBytes = Encoding.UTF8.GetBytes(configured);

        return providedBytes.Length == configuredBytes.Length
            && CryptographicOperations.FixedTimeEquals(providedBytes, configuredBytes);
    }
}
