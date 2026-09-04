using System.Text.Json;
using FluentAssertions;
using FluentAssertions.Execution;
using HalcyonRecords.Shared;

namespace HalcyonRecords.Api.UnitTests.Infrastructure.Seed;

public class SeedDataTests
{
    private const string SeedDataFolder = "Infrastructure/Seed/Data";

    private const int MinimumAlbumsPerGenre = 8;
    private const int MinimumAlbumsPerDecade = 10;

    private static readonly IReadOnlyList<ArtistSeedEntry> s_artists = Read<ArtistSeedEntry>(
        SeedDataFileNames.Artists
    );
    private static readonly IReadOnlyList<GenreSeedEntry> s_genres = Read<GenreSeedEntry>(
        SeedDataFileNames.Genres
    );
    private static readonly IReadOnlyList<DecadeSeedEntry> s_decades = Read<DecadeSeedEntry>(
        SeedDataFileNames.Decades
    );
    private static readonly IReadOnlyList<AlbumSeedEntry> s_albums = Read<AlbumSeedEntry>(
        SeedDataFileNames.Albums
    );

    private static List<T> Read<T>(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, SeedDataFolder, fileName);
        using var stream = File.OpenRead(path);

        return JsonSerializer.Deserialize<List<T>>(stream, SeedDataJsonOptions.Default)
            ?? throw new InvalidOperationException($"Seed file '{fileName}' deserialised to null.");
    }

    [Fact]
    public void Albums_ReferenceOnlyArtistsThatExist()
    {
        var artistSourceIds = s_artists.Select(artist => artist.SourceId).ToHashSet();

        var dangling = s_albums
            .SelectMany(album =>
                album
                    .ArtistSourceIds.Where(id => !artistSourceIds.Contains(id))
                    .Select(id => $"{album.Title} -> {id}")
            )
            .ToList();

        dangling.Should().BeEmpty();
    }

    [Fact]
    public void Albums_ReferenceOnlyGenresThatExist()
    {
        var genreSlugs = s_genres.Select(genre => genre.Slug).ToHashSet();

        var dangling = s_albums
            .SelectMany(album =>
                album
                    .GenreSlugs.Where(slug => !genreSlugs.Contains(slug))
                    .Select(slug => $"{album.Title} -> {slug}")
            )
            .ToList();

        dangling.Should().BeEmpty();
    }

    [Fact]
    public void Albums_HaveUniqueSourceIdsAndTitles()
    {
        s_albums.Select(album => album.SourceId).Should().OnlyHaveUniqueItems();
        s_albums.Select(album => album.Title).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Artists_HaveUniqueSourceIds()
    {
        s_artists.Select(artist => artist.SourceId).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Albums_HaveEveryFieldPopulated()
    {
        s_albums.Should().NotBeEmpty();

        using var scope = new AssertionScope();
        foreach (var album in s_albums)
        {
            album.Title.Should().NotBeNullOrWhiteSpace();
            album
                .Description.Should()
                .NotBeNullOrWhiteSpace($"'{album.Title}' needs a description");
            album.Label.Should().NotBeNullOrWhiteSpace($"'{album.Title}' needs a label");
            album.ImageUrl.Should().NotBeNullOrWhiteSpace($"'{album.Title}' needs an image");
            album.ReleaseDate.Should().NotBeNull($"'{album.Title}' needs a release date");
            album.ArtistSourceIds.Should().NotBeEmpty($"'{album.Title}' needs an artist");
            album.GenreSlugs.Should().NotBeEmpty($"'{album.Title}' needs a genre");
        }
    }

    [Fact]
    public void Artists_HaveEveryFieldPopulated()
    {
        s_artists.Should().NotBeEmpty();

        using var scope = new AssertionScope();
        foreach (var artist in s_artists)
        {
            artist.Name.Should().NotBeNullOrWhiteSpace();
            artist.Bio.Should().NotBeNullOrWhiteSpace($"'{artist.Name}' needs a bio");
            artist.Origin.Should().NotBeNullOrWhiteSpace($"'{artist.Name}' needs an origin");
            artist.ImageUrl.Should().NotBeNullOrWhiteSpace($"'{artist.Name}' needs an image");
            artist.Type.Should().NotBeNull($"'{artist.Name}' needs a type");
            artist.SinceYear.Should().NotBeNull($"'{artist.Name}' needs a since year");
        }
    }

    [Fact]
    public void Genres_HaveSlugsMatchingTheirNames()
    {
        using var scope = new AssertionScope();
        foreach (var genre in s_genres)
        {
            genre.Slug.Value.Should().Be(Slugifier.Slugify(genre.Name));
            genre.Description.Should().NotBeNullOrWhiteSpace($"'{genre.Name}' needs a description");
            genre.ImageUrl.Should().NotBeNullOrWhiteSpace($"'{genre.Name}' needs an image");
        }
    }

    [Fact]
    public void Decades_HaveEveryFieldPopulated()
    {
        using var scope = new AssertionScope();
        foreach (var decade in s_decades)
        {
            decade.Slug.Should().NotBeNullOrWhiteSpace();
            decade.Label.Should().NotBeNullOrWhiteSpace($"'{decade.Slug}' needs a label");
            decade
                .Description.Should()
                .NotBeNullOrWhiteSpace($"'{decade.Slug}' needs a description");
            decade.ImageUrl.Should().NotBeNullOrWhiteSpace($"'{decade.Slug}' needs an image");
            decade.EndYear.Should().NotBeNull($"'{decade.Slug}' needs an end year");
        }
    }

    [Fact]
    public void Albums_HaveSaneCommerceValues()
    {
        using var scope = new AssertionScope();
        foreach (var album in s_albums)
        {
            album.PriceInPence.Should().BePositive($"'{album.Title}' needs a price");
            album
                .UnitsInStock.Should()
                .BeGreaterThanOrEqualTo(0, $"'{album.Title}' cannot owe stock");

            if (album.OriginalPriceInPence is { } original)
            {
                original
                    .Should()
                    .BeGreaterThan(
                        album.PriceInPence,
                        $"'{album.Title}' is on sale, so its original price must be higher"
                    );
            }
        }
    }

    [Fact]
    public void Albums_AreNotCreditedToAnArtistBeforeThatArtistExisted()
    {
        var sinceYearBySourceId = s_artists.ToDictionary(
            artist => artist.SourceId,
            artist => artist.SinceYear
        );

        using var scope = new AssertionScope();
        foreach (var album in s_albums)
        {
            foreach (var artistSourceId in album.ArtistSourceIds)
            {
                if (
                    sinceYearBySourceId.TryGetValue(artistSourceId, out var sinceYear)
                    && sinceYear is { } since
                    && album.ReleaseDate is { } releaseDate
                )
                {
                    releaseDate
                        .Year.Should()
                        .BeGreaterThanOrEqualTo(
                            since,
                            $"'{album.Title}' cannot predate a credited artist"
                        );
                }
            }
        }
    }

    [Fact]
    public void EveryArtist_HasAtLeastOneAlbum()
    {
        var creditedArtistIds = s_albums.SelectMany(album => album.ArtistSourceIds).ToHashSet();

        var orphans = s_artists
            .Where(artist => !creditedArtistIds.Contains(artist.SourceId))
            .Select(artist => artist.Name)
            .ToList();

        orphans.Should().BeEmpty();
    }

    [Fact]
    public void EveryGenre_HasEnoughAlbumsToFillAnIndexPage()
    {
        using var scope = new AssertionScope();
        foreach (var genre in s_genres)
        {
            var count = s_albums.Count(album => album.GenreSlugs.Contains(genre.Slug));
            count
                .Should()
                .BeGreaterThanOrEqualTo(
                    MinimumAlbumsPerGenre,
                    $"'{genre.Name}' should not render a nearly empty index page"
                );
        }
    }

    [Fact]
    public void EveryDecade_HasEnoughAlbumsToFillAnIndexPage()
    {
        using var scope = new AssertionScope();
        foreach (var decade in s_decades)
        {
            var count = s_albums.Count(album =>
                album.ReleaseDate is { } releaseDate
                && (decade.StartYear is null || releaseDate.Year >= decade.StartYear)
                && (decade.EndYear is null || releaseDate.Year <= decade.EndYear)
            );

            count
                .Should()
                .BeGreaterThanOrEqualTo(
                    MinimumAlbumsPerDecade,
                    $"'{decade.Label}' should not render a nearly empty index page"
                );
        }
    }

    [Fact]
    public void EveryAlbum_FallsInsideExactlyOneDecadeBucket()
    {
        using var scope = new AssertionScope();
        foreach (var album in s_albums)
        {
            if (album.ReleaseDate is not { } releaseDate)
            {
                continue;
            }

            var matches = s_decades.Count(decade =>
                (decade.StartYear is null || releaseDate.Year >= decade.StartYear)
                && (decade.EndYear is null || releaseDate.Year <= decade.EndYear)
            );

            matches.Should().Be(1, $"'{album.Title}' must land in exactly one decade bucket");
        }
    }
}
