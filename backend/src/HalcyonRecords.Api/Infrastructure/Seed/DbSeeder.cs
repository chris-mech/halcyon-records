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

        var artists = artistEntries
            .Select(entry => new Artist
            {
                Name = entry.Name,
                Slug = entry.Slug,
                Bio = entry.Bio,
                Origin = entry.Origin,
                ActiveSince = entry.ActiveSince,
                ImageUrl = entry.ImageUrl,
            })
            .ToList();

        var genres = genreEntries
            .Select(entry => new Genre { Name = entry.Name, Slug = entry.Slug })
            .ToList();

        var artistsBySlug = artists.ToDictionary(artist => artist.Slug);
        var genresBySlug = genres.ToDictionary(genre => genre.Slug);

        var albums = albumEntries
            .Select(entry =>
            {
                var album = new Album
                {
                    Title = entry.Title,
                    Slug = entry.Slug,
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

                foreach (var artistSlug in entry.ArtistSlugs)
                {
                    album.AlbumArtists.Add(
                        new AlbumArtist { Album = album, Artist = artistsBySlug[artistSlug] }
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
