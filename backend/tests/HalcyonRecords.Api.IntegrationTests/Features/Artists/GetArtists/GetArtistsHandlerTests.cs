using FluentAssertions;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Artists.GetArtists;
using HalcyonRecords.Api.IntegrationTests.Common;
using HalcyonRecords.Shared;

namespace HalcyonRecords.Api.IntegrationTests.Features.Artists.GetArtists;

public class GetArtistsHandlerTests(SqlServerContainerFixture fixture)
    : IntegrationTestBase(fixture)
{
    private static readonly ArtistSqidEncoder s_artistSqids = new();

    private GetArtistsHandler Handler => new(DbContext, s_artistSqids);

    [Fact]
    public async Task Handle_ReturnsArtistsOrderedAlphabeticallyByName()
    {
        var zeta = NewArtist("Zeta Artist");
        var alpha = NewArtist("Alpha Artist");
        var mid = NewArtist("Mid Artist");

        DbContext.Artists.AddRange(zeta, alpha, mid);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(new GetArtistsQuery(), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result
            .Value.Select(a => a.Name)
            .Should()
            .ContainInOrder("Alpha Artist", "Mid Artist", "Zeta Artist");
    }

    [Fact]
    public async Task Handle_AlbumCount_ReflectsLinkedAlbums_AndIncludesZeroAlbumArtists()
    {
        var artistWithTwoAlbums = NewArtist("Artist With Two Albums");
        Link(artistWithTwoAlbums, NewAlbum("First Linked Album"));
        Link(artistWithTwoAlbums, NewAlbum("Second Linked Album"));

        var artistWithNoAlbums = NewArtist("Artist With No Albums");

        DbContext.Artists.AddRange(artistWithTwoAlbums, artistWithNoAlbums);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(new GetArtistsQuery(), CancellationToken.None);

        result.Value.Single(a => a.Name == "Artist With Two Albums").AlbumCount.Should().Be(2);
        result.Value.Single(a => a.Name == "Artist With No Albums").AlbumCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_MapsSqidAndNameSlugCorrectly()
    {
        var artist = NewArtist("Sqid Mapping Artist");
        DbContext.Artists.Add(artist);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(new GetArtistsQuery(), CancellationToken.None);

        var item = result.Value.Single();
        item.Sqid.Should().Be(s_artistSqids.Encode(artist.Id.Value));
        item.NameSlug.Should().Be(Slugifier.Slugify(artist.Name));
    }

    private static void Link(Artist artist, Album album) =>
        artist.AlbumArtists.Add(new AlbumArtist { Artist = artist, Album = album });

    private static Artist NewArtist(string name) => new() { Name = name };

    private static Album NewAlbum(string title) => new() { Title = title, PriceInPence = 1000 };
}
