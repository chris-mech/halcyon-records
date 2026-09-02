using System.Text.Json;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Infrastructure.Options;
using HalcyonRecords.Api.Infrastructure.Sql;
using HalcyonRecords.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.Infrastructure.Seed;

public static class DbSeeder
{
    private const string SeedDataFolder = "Infrastructure/Seed/Data";

    public static async Task SeedAsync(
        ApplicationDbContext dbContext,
        UserManager<User> userManager,
        IOptions<ShopOptions> shopOptions,
        TimeProvider timeProvider,
        CancellationToken cancellationToken = default
    )
    {
        if (await dbContext.Albums.AnyAsync(cancellationToken))
        {
            return;
        }

        var artistEntries = await ReadSeedFileAsync<ArtistSeedEntry>(
            SeedDataFileNames.Artists,
            cancellationToken
        );
        var genreEntries = await ReadSeedFileAsync<GenreSeedEntry>(
            SeedDataFileNames.Genres,
            cancellationToken
        );
        var decadeEntries = await ReadSeedFileAsync<DecadeSeedEntry>(
            SeedDataFileNames.Decades,
            cancellationToken
        );
        var albumEntries = await ReadSeedFileAsync<AlbumSeedEntry>(
            SeedDataFileNames.Albums,
            cancellationToken
        );

        var artistsBySourceId = artistEntries.ToDictionary(
            entry => entry.SourceId,
            entry => new Artist
            {
                Name = entry.Name,
                Bio = entry.Bio,
                Origin = entry.Origin,
                Type = entry.Type,
                SinceYear = entry.SinceYear,
                ImageUrl = entry.ImageUrl,
            }
        );
        var artists = artistsBySourceId.Values.ToList();

        var genres = genreEntries
            .Select(entry => new Genre
            {
                Name = entry.Name,
                Slug = entry.Slug.Value,
                Description = entry.Description,
                ImageUrl = entry.ImageUrl,
            })
            .ToList();

        var genresBySlug = genres.ToDictionary(genre => genre.Slug);

        var decades = decadeEntries
            .Select(entry => new Decade
            {
                Slug = entry.Slug,
                Label = entry.Label,
                StartYear = entry.StartYear,
                EndYear = entry.EndYear,
                Description = entry.Description,
                ImageUrl = entry.ImageUrl,
            })
            .ToList();

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
                    RestockUnitsInStock = entry.UnitsInStock,
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
                        new AlbumGenre { Album = album, Genre = genresBySlug[genreSlug.Value] }
                    );
                }

                return album;
            })
            .ToList();

        dbContext.Artists.AddRange(artists);
        dbContext.Genres.AddRange(genres);
        dbContext.Decades.AddRange(decades);
        dbContext.Albums.AddRange(albums);
        await dbContext.SaveChangesAsync(cancellationToken);

        await SeedShowcaseAccountAsync(
            dbContext,
            userManager,
            shopOptions,
            timeProvider,
            cancellationToken
        );
    }

    public const string ShowcaseAccountEmail = "demo@halcyonrecords.example";
    public const string ShowcaseAccountPassword = "DemoPassword123!";

    private static async Task SeedShowcaseAccountAsync(
        ApplicationDbContext dbContext,
        UserManager<User> userManager,
        IOptions<ShopOptions> shopOptions,
        TimeProvider timeProvider,
        CancellationToken cancellationToken
    )
    {
        var showcaseUser = new User
        {
            UserName = ShowcaseAccountEmail,
            Email = ShowcaseAccountEmail,
            FirstName = "Demo",
            LastName = "Shopper",
            RegisteredAt = timeProvider.GetUtcNow(),
            LastActiveAt = timeProvider.GetUtcNow(),
            IsShowcaseAccount = true,
        };

        var createResult = await userManager.CreateAsync(showcaseUser, ShowcaseAccountPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Failed to seed the fixed showcase account: "
                    + string.Join(", ", createResult.Errors.Select(e => e.Description))
            );
        }

        await ShowcaseAccountSeeder.SeedOrdersAsync(
            dbContext,
            showcaseUser,
            shopOptions,
            timeProvider,
            cancellationToken
        );
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
                SeedDataJsonOptions.Default,
                cancellationToken
            )
            ?? throw new InvalidOperationException($"Seed file '{fileName}' deserialised to null.");
    }
}
