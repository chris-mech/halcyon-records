using ErrorOr;
using FluentAssertions;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Artists.GetArtistById;
using HalcyonRecords.Api.IntegrationTests.Common;
using HalcyonRecords.Shared;

namespace HalcyonRecords.Api.IntegrationTests.Features.Artists.GetArtistById;

public class GetArtistByIdHandlerTests(SqlServerContainerFixture fixture)
    : IntegrationTestBase(fixture)
{
    private static readonly ArtistSqidEncoder s_artistSqids = new();
    private static readonly AlbumSqidEncoder s_albumSqids = new();

    private GetArtistByIdHandler Handler => new(DbContext, s_artistSqids, s_albumSqids);

    [Fact]
    public async Task Handle_ExistingArtist_ReturnsFullDetailWithDiscographyAndDistinctGenres()
    {
        var artist = new Artist
        {
            Name = "Full Detail Artist",
            Bio = "A short bio",
            Origin = "Bristol, UK",
            Type = ArtistType.Group,
            SinceYear = 2015,
            ImageUrl = "https://example.com/artist.jpg",
        };

        var album = NewAlbum("Solo Album", unitsInStock: 12);
        Link(album, artist);
        Link(album, NewGenre("Rock", "rock"));

        DbContext.Artists.Add(artist);
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetArtistByIdQuery(
                s_artistSqids.Encode(artist.Id.Value),
                nameof(ArtistAlbumSortBy.NewestFirst)
            ),
            CancellationToken.None
        );

        result.IsError.Should().BeFalse();
        var response = result.Value;
        response.Sqid.Should().Be(s_artistSqids.Encode(artist.Id.Value));
        response.Name.Should().Be("Full Detail Artist");
        response.NameSlug.Should().Be(Slugifier.Slugify("Full Detail Artist"));
        response.Bio.Should().Be("A short bio");
        response.Origin.Should().Be("Bristol, UK");
        response.Type.Should().Be(ArtistType.Group);
        response.SinceYear.Should().Be(2015);
        response.ImageUrl.Should().Be("https://example.com/artist.jpg");
        response.AlbumCount.Should().Be(1);
        response.Genres.Should().ContainSingle(g => g.Name == "Rock");
        response.Albums.Should().ContainSingle(a => a.Title == "Solo Album" && a.UnitsInStock == 12);
    }

    [Theory]
    [InlineData(nameof(ArtistAlbumSortBy.PriceAsc), "Cheaper Album", "Pricier Album")]
    [InlineData(nameof(ArtistAlbumSortBy.PriceDesc), "Pricier Album", "Cheaper Album")]
    public async Task Handle_PriceSort_OrdersDiscographyByPrice(
        string sort,
        string expectedFirstTitle,
        string expectedSecondTitle
    )
    {
        var artist = NewArtist("Price Sort Artist");
        var cheaper = NewAlbum("Cheaper Album", priceInPence: 500);
        var pricier = NewAlbum("Pricier Album", priceInPence: 1500);
        Link(cheaper, artist);
        Link(pricier, artist);

        DbContext.Artists.Add(artist);
        DbContext.Albums.AddRange(cheaper, pricier);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetArtistByIdQuery(s_artistSqids.Encode(artist.Id.Value), sort),
            CancellationToken.None
        );

        result
            .Value.Albums.Select(a => a.Title)
            .Should()
            .Equal(expectedFirstTitle, expectedSecondTitle);
    }

    [Theory]
    [InlineData(nameof(ArtistAlbumSortBy.NewestFirst), "Newer Album", "Older Album")]
    [InlineData(nameof(ArtistAlbumSortBy.OldestFirst), "Older Album", "Newer Album")]
    public async Task Handle_DateSort_OrdersDiscographyByReleaseDate(
        string sort,
        string expectedFirstTitle,
        string expectedSecondTitle
    )
    {
        var artist = NewArtist("Date Sort Artist");
        var older = NewAlbum("Older Album", releaseDate: new DateOnly(2010, 1, 1));
        var newer = NewAlbum("Newer Album", releaseDate: new DateOnly(2020, 1, 1));
        Link(older, artist);
        Link(newer, artist);

        DbContext.Artists.Add(artist);
        DbContext.Albums.AddRange(older, newer);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetArtistByIdQuery(s_artistSqids.Encode(artist.Id.Value), sort),
            CancellationToken.None
        );

        result
            .Value.Albums.Select(a => a.Title)
            .Should()
            .Equal(expectedFirstTitle, expectedSecondTitle);
    }

    [Fact]
    public async Task Handle_SharedGenreAcrossAlbums_AppearsOnlyOnceInTopLevelGenres()
    {
        var artist = NewArtist("Shared Genre Artist");
        var sharedGenre = NewGenre("Shared Genre", "shared-genre");

        var firstAlbum = NewAlbum("First Album");
        Link(firstAlbum, artist);
        Link(firstAlbum, sharedGenre);
        Link(firstAlbum, NewGenre("Only On First", "only-on-first"));

        var secondAlbum = NewAlbum("Second Album");
        Link(secondAlbum, artist);
        Link(secondAlbum, sharedGenre);

        DbContext.Artists.Add(artist);
        DbContext.Albums.AddRange(firstAlbum, secondAlbum);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetArtistByIdQuery(
                s_artistSqids.Encode(artist.Id.Value),
                nameof(ArtistAlbumSortBy.NewestFirst)
            ),
            CancellationToken.None
        );

        result.Value.Genres.Select(g => g.Name).Should().Equal("Only On First", "Shared Genre");
    }

    [Fact]
    public async Task Handle_ArtistWithNoAlbums_ReturnsZeroCountAndEmptyDiscography()
    {
        var artist = NewArtist("No Albums Artist");
        DbContext.Artists.Add(artist);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetArtistByIdQuery(
                s_artistSqids.Encode(artist.Id.Value),
                nameof(ArtistAlbumSortBy.NewestFirst)
            ),
            CancellationToken.None
        );

        result.IsError.Should().BeFalse();
        result.Value.AlbumCount.Should().Be(0);
        result.Value.Albums.Should().BeEmpty();
        result.Value.Genres.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_UnknownOrMalformedSqid_ReturnsNotFound(
        bool useWellFormedButUnknownSqid
    )
    {
        var sqid = useWellFormedButUnknownSqid ? s_artistSqids.Encode(999_999) : "not-a-real-sqid";

        var result = await Handler.Handle(
            new GetArtistByIdQuery(sqid, nameof(ArtistAlbumSortBy.NewestFirst)),
            CancellationToken.None
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    private static void Link(Album album, Artist artist) =>
        album.AlbumArtists.Add(new AlbumArtist { Album = album, Artist = artist });

    private static void Link(Album album, Genre genre) =>
        album.AlbumGenres.Add(new AlbumGenre { Album = album, Genre = genre });

    private static Artist NewArtist(string name) => new() { Name = name };

    private static Genre NewGenre(string name, string slug) => new() { Name = name, Slug = slug };

    private static Album NewAlbum(
        string title,
        int priceInPence = 1000,
        DateOnly? releaseDate = null,
        int unitsInStock = 0
    ) =>
        new()
        {
            Title = title,
            PriceInPence = priceInPence,
            ReleaseDate = releaseDate,
            UnitsInStock = unitsInStock,
        };
}
