using FluentAssertions;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Search.Search;
using HalcyonRecords.Api.Infrastructure.Search;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.IntegrationTests.Features.Search.Search;

public class SearchHandlerTests(
    SqlServerContainerFixture sqlFixture,
    MeilisearchContainerFixture meilisearchFixture
) : SearchIntegrationTestBase(sqlFixture, meilisearchFixture)
{
    private static readonly AlbumSqidEncoder s_albumSqids = new();
    private static readonly ArtistSqidEncoder s_artistSqids = new();

    private SearchHandler Handler =>
        new(
            MeilisearchClient,
            Options.Create(new MeilisearchIndexOptions()),
            Options.Create(new SearchOptions()),
            DbContext,
            s_albumSqids,
            s_artistSqids
        );

    [Fact]
    public async Task Handle_TitleMatch_ReturnsBestMatchAndGenreAdjacentSuggestions()
    {
        var jazzGenre = NewGenre("Search Jazz", "search-jazz");
        var folkGenre = NewGenre("Search Folk", "search-folk");

        var bestMatch = NewAlbum("Search Title Match Album");
        Link(bestMatch, NewArtist("Search Best Match Artist"));
        Link(bestMatch, jazzGenre);

        var sameGenre = NewAlbum("Search Same Genre Album");
        Link(sameGenre, NewArtist("Search Suggestion Artist"));
        Link(sameGenre, jazzGenre);

        var differentGenre = NewAlbum("Search Different Genre Album");
        Link(differentGenre, NewArtist("Search Unrelated Artist"));
        Link(differentGenre, folkGenre);

        DbContext.Albums.AddRange(bestMatch, sameGenre, differentGenre);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await Indexer.RebuildAsync(DbContext, TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new SearchQuery("Search Title Match Album"),
            CancellationToken.None
        );

        result.IsError.Should().BeFalse();
        result.Value.BestMatches.Should().ContainSingle(a => a.Title == "Search Title Match Album");
        result.Value.Suggestions.Should().ContainSingle(a => a.Title == "Search Same Genre Album");
        result
            .Value.Suggestions.Should()
            .NotContain(a => a.Title == "Search Different Genre Album");
        result.Value.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NoMatch_ReturnsEmptyResponse()
    {
        var album = NewAlbum("Search Unrelated Title Album");
        Link(album, NewArtist("Search Filler Artist"));
        Link(album, NewGenre("Search Filler Genre", "search-filler-genre"));

        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await Indexer.RebuildAsync(DbContext, TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new SearchQuery("zzyzxnonexistentquery"),
            CancellationToken.None
        );

        result.IsError.Should().BeFalse();
        result.Value.BestMatches.Should().BeEmpty();
        result.Value.Suggestions.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.SuggestedTerms.Should().HaveCount(3);
        result
            .Value.SuggestedTerms.Should()
            .OnlyContain(term =>
                term == "Search Unrelated Title Album"
                || term == "Search Filler Artist"
                || term == "Search Filler Genre"
            );
    }

    [Fact]
    public async Task Handle_NoMatch_WithLargerCatalog_ReturnsSuggestedTermsFromMixedPool()
    {
        var genreOne = NewGenre("Search Mix Genre One", "search-mix-genre-one");
        var genreTwo = NewGenre("Search Mix Genre Two", "search-mix-genre-two");
        var genreThree = NewGenre("Search Mix Genre Three", "search-mix-genre-three");

        var albumOne = NewAlbum("Search Mix Album One");
        Link(albumOne, NewArtist("Search Mix Artist One"));
        Link(albumOne, genreOne);

        var albumTwo = NewAlbum("Search Mix Album Two");
        Link(albumTwo, NewArtist("Search Mix Artist Two"));
        Link(albumTwo, genreTwo);

        var albumThree = NewAlbum("Search Mix Album Three");
        Link(albumThree, NewArtist("Search Mix Artist Three"));
        Link(albumThree, genreThree);

        DbContext.Albums.AddRange(albumOne, albumTwo, albumThree);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await Indexer.RebuildAsync(DbContext, TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new SearchQuery("nonexistentquery"),
            CancellationToken.None
        );

        var seededPool = new[]
        {
            "Search Mix Album One",
            "Search Mix Album Two",
            "Search Mix Album Three",
            "Search Mix Artist One",
            "Search Mix Artist Two",
            "Search Mix Artist Three",
            "Search Mix Genre One",
            "Search Mix Genre Two",
            "Search Mix Genre Three",
        };

        result.Value.SuggestedTerms.Should().HaveCount(3);
        result.Value.SuggestedTerms.Should().OnlyContain(term => seededPool.Contains(term));
    }

    [Fact]
    public async Task Handle_MultipleTitleMatches_ReturnsAllAsBestMatches()
    {
        var artist = NewArtist("Search Prolific Artist");
        var genre = NewGenre("Search Prolific Genre", "search-prolific-genre");

        var firstAlbum = NewAlbum("Search Prolific Artist Album One");
        var secondAlbum = NewAlbum("Search Prolific Artist Album Two");
        Link(firstAlbum, artist);
        Link(secondAlbum, artist);
        Link(firstAlbum, genre);
        Link(secondAlbum, genre);

        DbContext.Albums.AddRange(firstAlbum, secondAlbum);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        await Indexer.RebuildAsync(DbContext, TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new SearchQuery("Search Prolific Artist"),
            CancellationToken.None
        );

        result.Value.BestMatches.Should().HaveCount(2);
        result.Value.Suggestions.Should().BeEmpty();
    }

    private static void Link(Album album, Artist artist) =>
        album.AlbumArtists.Add(new AlbumArtist { Album = album, Artist = artist });

    private static void Link(Album album, Genre genre) =>
        album.AlbumGenres.Add(new AlbumGenre { Album = album, Genre = genre });

    private static Artist NewArtist(string name) => new() { Name = name };

    private static Genre NewGenre(string name, string slug) => new() { Name = name, Slug = slug };

    private static Album NewAlbum(string title) => new() { Title = title, PriceInPence = 1000 };
}
