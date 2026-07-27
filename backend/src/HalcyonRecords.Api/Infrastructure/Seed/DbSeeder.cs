using System.Text.Json;
using HalcyonRecords.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Infrastructure.Seed;

public static class DbSeeder
{
    private const string SeedDataFolder = "Infrastructure/Seed/Data";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken = default
    )
    {
        if (await dbContext.Albums.AnyAsync(cancellationToken))
        {
            return;
        }

        var artistEntries = await ReadSeedFileAsync<ArtistSeedEntry>(
            "SampleArtists.json",
            cancellationToken
        );
        var genreEntries = await ReadSeedFileAsync<GenreSeedEntry>(
            "SampleGenres.json",
            cancellationToken
        );
        var albumEntries = await ReadSeedFileAsync<AlbumSeedEntry>(
            "SampleAlbums.json",
            cancellationToken
        );

        var artistsBySourceId = artistEntries.ToDictionary(
            entry => entry.SourceId,
            entry => new Artist
            {
                Name = entry.Name,
                Bio = entry.Bio,
                Origin = entry.Origin,
                ActiveSince = entry.ActiveSince,
                ImageUrl = entry.ImageUrl,
            }
        );
        var artists = artistsBySourceId.Values.ToList();

        var genres = genreEntries
            .Select(entry => new Genre { Name = entry.Name, Slug = entry.Slug })
            .ToList();

        var genresBySlug = genres.ToDictionary(genre => genre.Slug);

        var albums = albumEntries
            .Select(entry =>
            {
                var album = new Album
                {
                    Title = entry.Title,
                    Description = entry.Description,
                    ReleaseDate = entry.ReleaseDate,
                    Label = entry.Label,
                    IsNew = entry.IsNew,
                    IsStaffPick = entry.IsStaffPick,
                    ImageUrl = entry.ImageUrl,
                    UnitsInStock = entry.UnitsInStock,
                    PriceInPence = entry.PriceInPence,
                    OriginalPriceInPence = entry.OriginalPriceInPence,
                };

                foreach (var artistSourceId in entry.ArtistSourceIds)
                {
                    album.AlbumArtists.Add(
                        new AlbumArtist
                        {
                            Album = album,
                            Artist = artistsBySourceId[artistSourceId],
                        }
                    );
                }

                foreach (var genreSlug in entry.GenreSlugs)
                {
                    album.AlbumGenres.Add(
                        new AlbumGenre { Album = album, Genre = genresBySlug[genreSlug] }
                    );
                }

                return album;
            })
            .ToList();

        dbContext.Artists.AddRange(artists);
        dbContext.Genres.AddRange(genres);
        dbContext.Albums.AddRange(albums);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<List<T>> ReadSeedFileAsync<T>(
        string fileName,
        CancellationToken cancellationToken
    )
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, SeedDataFolder, fileName);
        await using var stream = File.OpenRead(filePath);

        return await JsonSerializer.DeserializeAsync<List<T>>(
                stream,
                JsonOptions,
                cancellationToken
            )
            ?? throw new InvalidOperationException($"Seed file '{fileName}' deserialised to null.");
    }
}
