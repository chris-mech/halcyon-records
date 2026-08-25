using ErrorOr;
using FluentAssertions;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Genres.GetGenreBySlug;
using HalcyonRecords.Api.IntegrationTests.Common;

namespace HalcyonRecords.Api.IntegrationTests.Features.Genres.GetGenreBySlug;

public class GetGenreBySlugHandlerTests(SqlServerContainerFixture fixture)
    : IntegrationTestBase(fixture)
{
    private GetGenreBySlugHandler Handler => new(DbContext);

    [Fact]
    public async Task Handle_ExistingSlug_ReturnsGenreWithAlbumCount()
    {
        var genre = NewGenre("Detail Genre", "detail-genre");
        Link(genre, NewAlbum("Linked Album"));

        DbContext.Genres.Add(genre);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetGenreBySlugQuery("detail-genre"),
            CancellationToken.None
        );

        result.IsError.Should().BeFalse();
        result.Value.Name.Should().Be("Detail Genre");
        result.Value.AlbumCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_MapsImageUrlDirectly()
    {
        var genre = NewGenre("Mapped Genre", "mapped-genre");
        genre.ImageUrl = "https://example.com/mapped-genre.jpg";

        DbContext.Genres.Add(genre);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await Handler.Handle(
            new GetGenreBySlugQuery("mapped-genre"),
            CancellationToken.None
        );

        result.IsError.Should().BeFalse();
        result.Value.ImageUrl.Should().Be("https://example.com/mapped-genre.jpg");
    }

    [Fact]
    public async Task Handle_UnknownSlug_ReturnsNotFound()
    {
        var result = await Handler.Handle(
            new GetGenreBySlugQuery("missing-genre"),
            CancellationToken.None
        );

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    private static void Link(Genre genre, Album album) =>
        genre.AlbumGenres.Add(new AlbumGenre { Genre = genre, Album = album });

    private static Genre NewGenre(string name, string slug) => new() { Name = name, Slug = slug };

    private static Album NewAlbum(string title) => new() { Title = title, PriceInPence = 1000 };
}
