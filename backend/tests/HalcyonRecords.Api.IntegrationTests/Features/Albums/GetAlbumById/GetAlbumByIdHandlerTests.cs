using ErrorOr;
using FluentAssertions;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Albums.GetAlbumById;
using HalcyonRecords.Api.IntegrationTests.Common;
using HalcyonRecords.Shared;

namespace HalcyonRecords.Api.IntegrationTests.Features.Albums.GetAlbumById;

public class GetAlbumByIdHandlerTests(SqlServerContainerFixture fixture)
    : IntegrationTestBase(fixture)
{
    private static readonly AlbumSqidEncoder s_albumSqids = new();
    private static readonly ArtistSqidEncoder s_artistSqids = new();

    private GetAlbumByIdHandler Handler => new(DbContext, s_albumSqids, s_artistSqids);

    [Fact]
    public async Task Handle_ExistingAlbum_ReturnsFullDetailWithOrderedArtistsAndGenres()
    {
        var album = new Album
        {
            Title = "Full Detail Album",
            Description = "A description",
            Label = "Halcyon",
            ImageUrl = "https://example.com/cover.jpg",
            ReleaseDate = new DateOnly(2020, 1, 1),
            PriceInPence = 1000,
            OriginalPriceInPence = 1500,
            IsNew = true,
            IsStaffPick = true,
            UnitsInStock = 3,
        };
        var alphaBand = NewArtist("Alpha Band");
        var zetaBand = NewArtist("Zeta Band");
        Link(album, zetaBand);
        Link(album, alphaBand);
        Link(album, NewGenre("Rock", "rock"));
        Link(album, NewGenre("Ambient", "ambient"));

        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetAlbumByIdQuery(s_albumSqids.Encode(album.Id.Value)),
            CancellationToken.None
        );

        result.IsError.Should().BeFalse();
        var response = result.Value;
        response.Sqid.Should().Be(s_albumSqids.Encode(album.Id.Value));
        response.Title.Should().Be("Full Detail Album");
        response.TitleSlug.Should().Be(Slugifier.Slugify("Full Detail Album"));
        response.Description.Should().Be("A description");
        response.Label.Should().Be("Halcyon");
        response.ImageUrl.Should().Be("https://example.com/cover.jpg");
        response.ReleaseDate.Should().Be(new DateOnly(2020, 1, 1));
        response.IsOnSale.Should().BeTrue();
        response.IsInStock.Should().BeTrue();
        response.Artists.Select(a => a.Name).Should().Equal("Alpha Band", "Zeta Band");
        response
            .Artists.Select(a => a.Sqid)
            .Should()
            .Equal(
                s_artistSqids.Encode(alphaBand.Id.Value),
                s_artistSqids.Encode(zetaBand.Id.Value)
            );
        response.Artists.Select(a => a.NameSlug).Should().Equal("alpha-band", "zeta-band");
        response.Genres.Select(g => g.Name).Should().Equal("Ambient", "Rock");
    }

    [Fact]
    public async Task Handle_AlbumNotOnSaleAndOutOfStock_DerivesFalseForBothFlags()
    {
        var album = new Album
        {
            Title = "Full Price Out Of Stock",
            PriceInPence = 1000,
            OriginalPriceInPence = null,
            UnitsInStock = 0,
        };

        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetAlbumByIdQuery(s_albumSqids.Encode(album.Id.Value)),
            CancellationToken.None
        );

        result.Value.IsOnSale.Should().BeFalse();
        result.Value.IsInStock.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_UnknownOrMalformedSqid_ReturnsNotFound(
        bool useWellFormedButUnknownSqid
    )
    {
        var sqid = useWellFormedButUnknownSqid ? s_albumSqids.Encode(999_999) : "not-a-real-sqid";

        var result = await Handler.Handle(new GetAlbumByIdQuery(sqid), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    private static void Link(Album album, Artist artist) =>
        album.AlbumArtists.Add(new AlbumArtist { Album = album, Artist = artist });

    private static void Link(Album album, Genre genre) =>
        album.AlbumGenres.Add(new AlbumGenre { Album = album, Genre = genre });

    private static Artist NewArtist(string name) => new() { Name = name };

    private static Genre NewGenre(string name, string slug) => new() { Name = name, Slug = slug };
}
