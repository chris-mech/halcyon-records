using FluentAssertions;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Albums.GetAlbums;
using HalcyonRecords.Api.IntegrationTests.Common;
using Sqids;

namespace HalcyonRecords.Api.IntegrationTests.Features.Albums.GetAlbums;

public class GetAlbumsHandlerTests(SqlServerContainerFixture fixture) : IntegrationTestBase(fixture)
{
    private GetAlbumsHandler Handler => new(DbContext, new AlbumSqidEncoder(), new ArtistSqidEncoder());

    [Fact]
    public async Task Handle_AlbumWithMultipleArtistsAndGenres_ReturnsAllOfThemNested()
    {
        var album = NewAlbum("Concrete Bloom");
        Link(album, NewArtist("Artist One"));
        Link(album, NewArtist("Artist Two"));
        Link(album, NewGenre("Genre One", "genre-one"));
        Link(album, NewGenre("Genre Two", "genre-two"));

        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(CreateQuery(), CancellationToken.None);

        var item = result.Value.Items.Should().ContainSingle().Subject;
        item.Artists.Select(a => a.Name).Should().BeEquivalentTo("Artist One", "Artist Two");
        item.Genres.Select(g => g.Slug).Should().BeEquivalentTo("genre-one", "genre-two");
    }

    [Theory]
    [InlineData(nameof(AlbumSortBy.ArtistAZ), "Album With Alpha First", "Album With Mid Only")]
    [InlineData(nameof(AlbumSortBy.ArtistZA), "Album With Mid Only", "Album With Alpha First")]
    public async Task Handle_ArtistSort_UsesAlphabeticallyFirstCreditedArtistPerAlbum(
        string sort,
        string expectedFirstTitle,
        string expectedSecondTitle
    )
    {
        var multiArtistAlbum = NewAlbum("Album With Alpha First");
        Link(multiArtistAlbum, NewArtist("Zeta Band"));
        Link(multiArtistAlbum, NewArtist("Alpha Band"));

        var singleArtistAlbum = NewAlbum("Album With Mid Only");
        Link(singleArtistAlbum, NewArtist("Mid Band"));

        DbContext.Albums.AddRange(multiArtistAlbum, singleArtistAlbum);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(CreateQuery(sort: sort), CancellationToken.None);

        result
            .Value.Items.Select(a => a.Title)
            .Should()
            .Equal(expectedFirstTitle, expectedSecondTitle);
    }

    [Fact]
    public async Task Handle_GenresFilter_MatchesAlbumsWithAnySelectedGenre_NotAll()
    {
        var rockAlbum = NewAlbum("Rock Album");
        Link(rockAlbum, NewGenre("Rock", "rock"));

        var jazzAlbum = NewAlbum("Jazz Album");
        Link(jazzAlbum, NewGenre("Jazz", "jazz"));

        var folkAlbum = NewAlbum("Folk Album");
        Link(folkAlbum, NewGenre("Folk", "folk"));

        DbContext.Albums.AddRange(rockAlbum, jazzAlbum, folkAlbum);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            CreateQuery(genres: ["rock", "jazz"]),
            CancellationToken.None
        );

        result.Value.Items.Select(a => a.Title).Should().BeEquivalentTo("Rock Album", "Jazz Album");
    }

    [Fact]
    public async Task Handle_PageBeyondTotalPages_ReturnsEmptyItemsNotError()
    {
        DbContext.Albums.Add(NewAlbum("Only Album"));
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            CreateQuery(page: 5, pageSize: 12),
            CancellationToken.None
        );

        result.IsError.Should().BeFalse();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NonEvenlyDivisibleAlbumCount_ReportsTotalCountFromWholeSet_NotThePage()
    {
        for (var i = 1; i <= 7; i++)
        {
            DbContext.Albums.Add(NewAlbum($"Album {i}"));
        }
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            CreateQuery(page: 1, pageSize: 3),
            CancellationToken.None
        );

        result.Value.TotalCount.Should().Be(7);
        result.Value.TotalPages.Should().Be(3);
        result.Value.Items.Should().HaveCount(3);
    }

    [Theory]
    [InlineData(true, false, false, false, "New Album")]
    [InlineData(false, true, false, false, "On Sale Album")]
    [InlineData(false, false, true, false, "Staff Pick Album")]
    [InlineData(false, false, false, true, "In Stock Album")]
    public async Task Handle_BoolFilter_NarrowsToOnlyMatchingAlbums(
        bool isNew,
        bool isOnSale,
        bool isStaffPick,
        bool inStock,
        string expectedTitle
    )
    {
        DbContext.Albums.AddRange(
            NewAlbum("New Album", isNew: true),
            NewAlbum("On Sale Album", priceInPence: 1000, originalPriceInPence: 1500),
            NewAlbum("Staff Pick Album", isStaffPick: true),
            NewAlbum("In Stock Album", unitsInStock: 5),
            NewAlbum("Plain Album")
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            CreateQuery(
                isNew: isNew,
                isOnSale: isOnSale,
                isStaffPick: isStaffPick,
                inStock: inStock
            ),
            CancellationToken.None
        );

        result.Value.Items.Should().ContainSingle(a => a.Title == expectedTitle);
    }

    [Fact]
    public async Task Handle_AlbumResponse_DerivesIsOnSaleAndIsInStockFromUnderlyingData()
    {
        DbContext.Albums.AddRange(
            NewAlbum(
                "On Sale In Stock",
                priceInPence: 1000,
                originalPriceInPence: 1500,
                unitsInStock: 3
            ),
            NewAlbum("Full Price Out Of Stock", priceInPence: 1000)
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(CreateQuery(), CancellationToken.None);

        var onSale = result.Value.Items.Single(a => a.Title == "On Sale In Stock");
        onSale.IsOnSale.Should().BeTrue();
        onSale.IsInStock.Should().BeTrue();

        var fullPrice = result.Value.Items.Single(a => a.Title == "Full Price Out Of Stock");
        fullPrice.IsOnSale.Should().BeFalse();
        fullPrice.IsInStock.Should().BeFalse();
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
        int? originalPriceInPence = null,
        bool isNew = false,
        bool isStaffPick = false,
        int unitsInStock = 0
    ) =>
        new()
        {
            Title = title,
            PriceInPence = priceInPence,
            OriginalPriceInPence = originalPriceInPence,
            IsNew = isNew,
            IsStaffPick = isStaffPick,
            UnitsInStock = unitsInStock,
        };

    private static GetAlbumsQuery CreateQuery(
        int page = 1,
        int pageSize = 12,
        bool isNew = false,
        bool isOnSale = false,
        bool isStaffPick = false,
        bool inStock = false,
        IReadOnlyList<string>? genres = null,
        string sort = nameof(AlbumSortBy.NewestFirst)
    ) => new(page, pageSize, isNew, isOnSale, isStaffPick, inStock, genres, sort);
}
