using System.Text.Json.Serialization;
using Meilisearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.Infrastructure.Search;

public sealed record AlbumSearchDocument(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("artists")] IReadOnlyList<string> Artists,
    [property: JsonPropertyName("genres")] IReadOnlyList<string> Genres,
    [property: JsonPropertyName("releaseYear")] string? ReleaseYear
);

public sealed class MeilisearchIndexer(
    MeilisearchClient client,
    IOptions<MeilisearchIndexOptions> options
)
{
    private const double TaskTimeoutMs = 30_000;

    private static readonly string[] s_searchableAttributes =
    [
        "title",
        "artists",
        "genres",
        "releaseYear",
    ];
    private static readonly string[] s_filterableAttributes = ["genres", "id"];

    public async Task RebuildAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default
    )
    {
        var index = client.Index(options.Value.IndexName);

        await RunAndWaitAsync(
            index,
            () => index.UpdateSearchableAttributesAsync(s_searchableAttributes, cancellationToken),
            cancellationToken
        );
        await RunAndWaitAsync(
            index,
            () => index.UpdateFilterableAttributesAsync(s_filterableAttributes, cancellationToken),
            cancellationToken
        );

        var rows = await dbContext
            .Albums.Select(a => new
            {
                a.Id,
                a.Title,
                a.ReleaseDate,
                Artists = a.AlbumArtists.Select(aa => aa.Artist.Name),
                Genres = a.AlbumGenres.Select(ag => ag.Genre.Name),
            })
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        var documents = rows.Select(a => new AlbumSearchDocument(
            a.Id.Value,
            a.Title,
            a.Artists.ToList(),
            a.Genres.ToList(),
            a.ReleaseDate.HasValue ? a.ReleaseDate.Value.Year.ToString() : null
        ));

        await RunAndWaitAsync(
            index,
            () =>
                index.AddDocumentsAsync(
                    documents,
                    primaryKey: "id",
                    cancellationToken: cancellationToken
                ),
            cancellationToken
        );
    }

    private static async Task RunAndWaitAsync(
        Meilisearch.Index index,
        Func<Task<TaskInfo>> operation,
        CancellationToken cancellationToken
    )
    {
        var task = await operation();
        var completed = await index.WaitForTaskAsync(
            task.TaskUid,
            TaskTimeoutMs,
            cancellationToken: cancellationToken
        );

        if (completed.Status != TaskInfoStatus.Succeeded)
        {
            throw new InvalidOperationException(
                $"Meilisearch task {completed.Uid} ({completed.Type}) did not succeed: {completed.Status}."
            );
        }
    }
}
