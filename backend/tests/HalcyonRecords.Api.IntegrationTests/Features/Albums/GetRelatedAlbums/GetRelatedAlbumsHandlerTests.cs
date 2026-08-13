using ErrorOr;
using FluentAssertions;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Albums.GetRelatedAlbums;
using HalcyonRecords.Api.IntegrationTests.Common;
using HalcyonRecords.Shared;

namespace HalcyonRecords.Api.IntegrationTests.Features.Albums.GetRelatedAlbums;

public class GetRelatedAlbumsHandlerTests(SqlServerContainerFixture fixture)
    : IntegrationTestBase(fixture)
{
    private static readonly AlbumSqidEncoder s_albumSqids = new();
    private static readonly ArtistSqidEncoder s_artistSqids = new();

    private const int MaxResults = 4;

    private GetRelatedAlbumsHandler Handler => new(DbContext, s_albumSqids, s_artistSqids);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_UnknownOrMalformedSqid_ReturnsNotFound(
        bool useWellFormedButUnknownSqid
    )
    {
        var sqid = useWellFormedButUnknownSqid ? s_albumSqids.Encode(999_999) : "not-a-real-sqid";

        var result = await Handler.Handle(new GetRelatedAlbumsQuery(sqid), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_GenreMatches_TakePriorityOverArtistMatches_AndCapsAtMaxResults()
    {
        var sharedGenre = NewGenre("Shared Genre", "shared-genre");
        var currentArtist = NewArtist("Current Artist");

        var current = NewAlbum("Current Album");
        Link(current, sharedGenre);
        Link(current, currentArtist);

        var genreMatches = Enumerable
            .Range(1, MaxResults + 2)
            .Select(i => NewAlbum($"Genre Match {i}"))
            .ToList();
        foreach (var album in genreMatches)
        {
            Link(album, sharedGenre);
        }

        var artistOnlyMatches = Enumerable
            .Range(1, 3)
            .Select(i => NewAlbum($"Artist Only Match {i}"))
            .ToList();
        foreach (var album in artistOnlyMatches)
        {
            Link(album, currentArtist);
        }

        List<Album> albums = [current, .. genreMatches, .. artistOnlyMatches];
        DbContext.Albums.AddRange(albums);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetRelatedAlbumsQuery(s_albumSqids.Encode(current.Id.Value)),
            CancellationToken.None
        );

        result.Value.Should().HaveCount(MaxResults);
        result.Value.Select(a => a.Title).Should().BeSubsetOf(genreMatches.Select(a => a.Title));
    }

    [Fact]
    public async Task Handle_ArtistMatches_FillRemainingSlots_WhenGenreMatchesAreFewerThanMaxResults()
    {
        var rareGenre = NewGenre("Rare Genre", "rare-genre");
        var rareArtist = NewArtist("Rare Artist");

        var current = NewAlbum("Current Album");
        Link(current, rareGenre);
        Link(current, rareArtist);

        var genreMatches = new[] { NewAlbum("Genre Match 1"), NewAlbum("Genre Match 2") };
        foreach (var album in genreMatches)
        {
            Link(album, rareGenre);
        }

        var artistMatches = new[]
        {
            NewAlbum("Artist Match 1"),
            NewAlbum("Artist Match 2"),
            NewAlbum("Artist Match 3"),
        };
        foreach (var album in artistMatches)
        {
            Link(album, rareArtist);
        }

        List<Album> albums = [current, .. genreMatches, .. artistMatches];
        DbContext.Albums.AddRange(albums);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetRelatedAlbumsQuery(s_albumSqids.Encode(current.Id.Value)),
            CancellationToken.None
        );

        var titles = result.Value.Select(a => a.Title).ToList();
        var remainingSlots = MaxResults - genreMatches.Length;

        titles.Should().HaveCount(MaxResults);
        titles.Should().Contain(genreMatches.Select(a => a.Title));
        titles.Intersect(artistMatches.Select(a => a.Title)).Should().HaveCount(remainingSlots);

        var albumsByTitle = albums.ToDictionary(a => a.Title);
        foreach (var item in result.Value)
        {
            item.Sqid.Should().Be(s_albumSqids.Encode(albumsByTitle[item.Title].Id.Value));
            item.TitleSlug.Should().Be(Slugifier.Slugify(item.Title));
            item.IsOnSale.Should().BeFalse();
            item.IsInStock.Should().BeFalse();
        }

        var pickedArtistMatch = result.Value.First(a => artistMatches.Any(m => m.Title == a.Title));
        pickedArtistMatch.Artists.Should().ContainSingle();
        pickedArtistMatch.Artists[0].Name.Should().Be("Rare Artist");
        pickedArtistMatch.Artists[0].Sqid.Should().Be(s_artistSqids.Encode(rareArtist.Id.Value));
        pickedArtistMatch.Artists[0].NameSlug.Should().Be("rare-artist");
    }

    [Fact]
    public async Task Handle_CandidateMatchingBothGenreAndArtist_AppearsOnlyOnce()
    {
        var sharedGenre = NewGenre("Overlap Genre", "overlap-genre");
        var sharedArtist = NewArtist("Overlap Artist");

        var current = NewAlbum("Current Album");
        Link(current, sharedGenre);
        Link(current, sharedArtist);

        var bothMatch = NewAlbum("Both Match");
        Link(bothMatch, sharedGenre);
        Link(bothMatch, sharedArtist);

        var genreOnlyMatches = new[] { NewAlbum("Genre Only 1"), NewAlbum("Genre Only 2") };
        foreach (var album in genreOnlyMatches)
        {
            Link(album, sharedGenre);
        }

        List<Album> albums = [current, bothMatch, .. genreOnlyMatches];
        DbContext.Albums.AddRange(albums);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetRelatedAlbumsQuery(s_albumSqids.Encode(current.Id.Value)),
            CancellationToken.None
        );

        result.Value.Should().HaveCount(3);
        result.Value.Select(a => a.Sqid).Should().OnlyHaveUniqueItems();
        result.Value.Should().ContainSingle(a => a.Title == "Both Match");
    }

    [Fact]
    public async Task Handle_FallsBackToReleaseDateProximity_WhenNoGenreOrArtistMatchExists()
    {
        var releaseDate = new DateOnly(2020, 1, 1);
        var current = NewAlbum("Current Album", releaseDate: releaseDate);
        Link(current, NewGenre("Unmatched Genre", "unmatched-genre"));
        Link(current, NewArtist("Unmatched Artist"));

        var closest = Enumerable
            .Range(1, MaxResults)
            .Select(i => NewAlbum($"Plus {i} Days", releaseDate: releaseDate.AddDays(i)))
            .ToList();
        var farthest = NewAlbum("Plus Far Days", releaseDate: releaseDate.AddDays(1000));

        List<Album> albums = [current, .. closest, farthest];
        DbContext.Albums.AddRange(albums);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetRelatedAlbumsQuery(s_albumSqids.Encode(current.Id.Value)),
            CancellationToken.None
        );

        var titles = result.Value.Select(a => a.Title).ToList();
        titles.Should().HaveCount(MaxResults);
        titles.Should().BeEquivalentTo(closest.Select(a => a.Title));
        titles.Should().NotContain(farthest.Title);
    }

    [Fact]
    public async Task Handle_NoOtherAlbumsExist_ReturnsEmptyListNotError()
    {
        var current = NewAlbum("Only Album");
        DbContext.Albums.Add(current);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetRelatedAlbumsQuery(s_albumSqids.Encode(current.Id.Value)),
            CancellationToken.None
        );

        result.IsError.Should().BeFalse();
        result.Value.Should().BeEmpty();
    }

    private static void Link(Album album, Artist artist) =>
        album.AlbumArtists.Add(new AlbumArtist { Album = album, Artist = artist });

    private static void Link(Album album, Genre genre) =>
        album.AlbumGenres.Add(new AlbumGenre { Album = album, Genre = genre });

    private static Artist NewArtist(string name) => new() { Name = name };

    private static Genre NewGenre(string name, string slug) => new() { Name = name, Slug = slug };

    private static Album NewAlbum(string title, DateOnly? releaseDate = null) =>
        new()
        {
            Title = title,
            PriceInPence = 1000,
            ReleaseDate = releaseDate,
        };
}
