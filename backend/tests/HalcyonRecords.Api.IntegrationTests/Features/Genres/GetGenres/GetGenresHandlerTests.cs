using FluentAssertions;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Genres.GetGenres;
using HalcyonRecords.Api.IntegrationTests.Common;

namespace HalcyonRecords.Api.IntegrationTests.Features.Genres.GetGenres;

public class GetGenresHandlerTests(SqlServerContainerFixture fixture) : IntegrationTestBase(fixture)
{
    private GetGenresHandler Handler => new(DbContext);

    [Fact]
    public async Task Handle_ReturnsGenresOrderedAlphabeticallyByName()
    {
        DbContext.Genres.AddRange(
            NewGenre("Zeta Genre", "zeta-genre"),
            NewGenre("Alpha Genre", "alpha-genre"),
            NewGenre("Mid Genre", "mid-genre")
        );
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(new GetGenresQuery(), CancellationToken.None);

        result.IsError.Should().BeFalse();
        result
            .Value.Select(g => g.Name)
            .Should()
            .ContainInOrder("Alpha Genre", "Mid Genre", "Zeta Genre");
    }

    [Fact]
    public async Task Handle_AlbumCount_ReflectsLinkedAlbums_AndIncludesZeroAlbumGenres()
    {
        var genreWithTwoAlbums = NewGenre("Genre With Two Albums", "genre-with-two-albums");
        Link(genreWithTwoAlbums, NewAlbum("First Linked Album"));
        Link(genreWithTwoAlbums, NewAlbum("Second Linked Album"));

        var genreWithNoAlbums = NewGenre("Genre With No Albums", "genre-with-no-albums");

        DbContext.Genres.AddRange(genreWithTwoAlbums, genreWithNoAlbums);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(new GetGenresQuery(), CancellationToken.None);

        result.Value.Single(g => g.Name == "Genre With Two Albums").AlbumCount.Should().Be(2);
        result.Value.Single(g => g.Name == "Genre With No Albums").AlbumCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_MapsImageUrlDirectly()
    {
        var genre = NewGenre("Mapped Genre", "mapped-genre");
        genre.ImageUrl = "https://example.com/mapped-genre.jpg";

        DbContext.Genres.Add(genre);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var item = (
            await Handler.Handle(new GetGenresQuery(), CancellationToken.None)
        ).Value.Single();

        item.ImageUrl.Should().Be("https://example.com/mapped-genre.jpg");
    }

    private static void Link(Genre genre, Album album) =>
        genre.AlbumGenres.Add(new AlbumGenre { Genre = genre, Album = album });

    private static Genre NewGenre(string name, string slug) => new() { Name = name, Slug = slug };

    private static Album NewAlbum(string title) => new() { Title = title, PriceInPence = 1000 };
}
