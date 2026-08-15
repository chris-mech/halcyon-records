using FluentAssertions;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Search;
using HalcyonRecords.Api.Features.Search.GetSearchSuggestions;
using HalcyonRecords.Api.Features.Search.Search;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.IntegrationTests.Features.Search.GetSearchSuggestions;

public class GetSearchSuggestionsHandlerTests(SqlServerContainerFixture fixture)
    : IntegrationTestBase(fixture)
{
    private GetSearchSuggestionsHandler Handler =>
        new(new SuggestedTermsProvider(Options.Create(new SearchOptions())), DbContext);

    [Fact]
    public async Task Handle_ReturnsThreeTerms_FromMixedPool()
    {
        var genre = NewGenre("Suggestions Pool Genre", "suggestions-pool-genre");
        var artist = NewArtist("Suggestions Pool Artist");
        var album = NewAlbum("Suggestions Pool Album");
        Link(album, artist);
        Link(album, genre);

        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(new GetSearchSuggestionsQuery(), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result.Value.Should().HaveCount(3);
        result
            .Value.Should()
            .OnlyContain(term =>
                term == "Suggestions Pool Album"
                || term == "Suggestions Pool Artist"
                || term == "Suggestions Pool Genre"
            );
    }

    [Fact]
    public async Task Handle_WithLargerCatalog_ReturnsTermsFromMixedPool()
    {
        var genreOne = NewGenre("Suggestions Mix Genre One", "suggestions-mix-genre-one");
        var genreTwo = NewGenre("Suggestions Mix Genre Two", "suggestions-mix-genre-two");

        var albumOne = NewAlbum("Suggestions Mix Album One");
        Link(albumOne, NewArtist("Suggestions Mix Artist One"));
        Link(albumOne, genreOne);

        var albumTwo = NewAlbum("Suggestions Mix Album Two");
        Link(albumTwo, NewArtist("Suggestions Mix Artist Two"));
        Link(albumTwo, genreTwo);

        DbContext.Albums.AddRange(albumOne, albumTwo);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(new GetSearchSuggestionsQuery(), CancellationToken.None);

        var seededPool = new[]
        {
            "Suggestions Mix Album One",
            "Suggestions Mix Album Two",
            "Suggestions Mix Artist One",
            "Suggestions Mix Artist Two",
            "Suggestions Mix Genre One",
            "Suggestions Mix Genre Two",
        };

        result.Value.Should().HaveCount(3);
        result.Value.Should().OnlyContain(term => seededPool.Contains(term));
    }

    private static void Link(Album album, Artist artist) =>
        album.AlbumArtists.Add(new AlbumArtist { Album = album, Artist = artist });

    private static void Link(Album album, Genre genre) =>
        album.AlbumGenres.Add(new AlbumGenre { Album = album, Genre = genre });

    private static Artist NewArtist(string name) => new() { Name = name };

    private static Genre NewGenre(string name, string slug) => new() { Name = name, Slug = slug };

    private static Album NewAlbum(string title) => new() { Title = title, PriceInPence = 1000 };
}
