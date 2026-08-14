using System.Data.Common;

namespace HalcyonRecords.Api.Infrastructure.Search;

public sealed record MeilisearchClientOptions(Uri Endpoint, string? MasterKey)
{
    public static MeilisearchClientOptions Parse(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No Meilisearch connection string was found. Ensure the 'meilisearch' resource is referenced in AppHost."
            );
        }

        if (Uri.TryCreate(connectionString, UriKind.Absolute, out var bareUri))
        {
            return new MeilisearchClientOptions(bareUri, MasterKey: null);
        }

        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };

        if (
            !builder.TryGetValue("Endpoint", out var endpointValue)
            || !Uri.TryCreate(endpointValue.ToString(), UriKind.Absolute, out var endpoint)
        )
        {
            throw new InvalidOperationException(
                "The Meilisearch connection string did not contain a valid 'Endpoint' value."
            );
        }

        var masterKey = builder.TryGetValue("MasterKey", out var masterKeyValue)
            ? masterKeyValue.ToString()
            : null;

        return new MeilisearchClientOptions(endpoint, masterKey);
    }
}
