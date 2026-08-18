using ErrorOr;
using FluentAssertions;
using HalcyonRecords.Api.Common.Sqids;
using HalcyonRecords.Api.Domain;
using HalcyonRecords.Api.Features.Albums.GetCoverStory;
using HalcyonRecords.Api.IntegrationTests.Common;
using Microsoft.Extensions.Time.Testing;

namespace HalcyonRecords.Api.IntegrationTests.Features.Albums.GetCoverStory;

public class GetCoverStoryHandlerTests(SqlServerContainerFixture fixture)
    : IntegrationTestBase(fixture)
{
    private static readonly DateOnly s_epoch = new(2026, 8, 10);

    private GetCoverStoryHandler HandlerOn(DateOnly today) =>
        new(
            DbContext,
            new AlbumSqidEncoder(),
            new ArtistSqidEncoder(),
            new FakeTimeProvider(
                new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            )
        );

    [Fact]
    public async Task Handle_NoStaffPicks_FallsBackToNewestAlbumByReleaseDate()
    {
        var older = NewAlbum("Older Album", new DateOnly(2020, 1, 1));
        var newer = NewAlbum("Newer Album", new DateOnly(2023, 6, 1));
        DbContext.Albums.AddRange(older, newer);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await HandlerOn(s_epoch)
            .Handle(new GetCoverStoryQuery(), CancellationToken.None);

        result.Value.Title.Should().Be("Newer Album");
    }

    [Fact]
    public async Task Handle_NoAlbumsAtAll_ReturnsNotFound()
    {
        var result = await HandlerOn(s_epoch)
            .Handle(new GetCoverStoryQuery(), CancellationToken.None);

        result.IsError.Should().BeTrue();
        result.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_OnEpochWeek_PicksFirstStaffPickByIdOrder()
    {
        var picks = Enumerable
            .Range(1, 3)
            .Select(i =>
                NewAlbum($"Staff Pick {i}", isStaffPick: true, unitsInStock: i == 1 ? 7 : 0)
            )
            .ToList();
        DbContext.Albums.AddRange(picks);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await HandlerOn(s_epoch)
            .Handle(new GetCoverStoryQuery(), CancellationToken.None);

        result.Value.Title.Should().Be("Staff Pick 1");
        result.Value.IssueNumber.Should().Be(1);
        result.Value.UnitsInStock.Should().Be(7);
    }

    [Fact]
    public async Task Handle_OneWeekLater_AdvancesToNextStaffPick()
    {
        var picks = Enumerable
            .Range(1, 3)
            .Select(i => NewAlbum($"Staff Pick {i}", isStaffPick: true))
            .ToList();
        DbContext.Albums.AddRange(picks);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await HandlerOn(s_epoch.AddDays(7))
            .Handle(new GetCoverStoryQuery(), CancellationToken.None);

        result.Value.Title.Should().Be("Staff Pick 2");
        result.Value.IssueNumber.Should().Be(2);
    }

    [Fact]
    public async Task Handle_RotationWrapsAround_WhenWeekIndexExceedsPoolSize()
    {
        var picks = Enumerable
            .Range(1, 3)
            .Select(i => NewAlbum($"Staff Pick {i}", isStaffPick: true))
            .ToList();
        DbContext.Albums.AddRange(picks);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await HandlerOn(s_epoch.AddDays(21))
            .Handle(new GetCoverStoryQuery(), CancellationToken.None);

        result.Value.Title.Should().Be("Staff Pick 1");
        result.Value.IssueNumber.Should().Be(4);
    }

    [Fact]
    public async Task Handle_MapsAllGenres_NotJustTheFirst()
    {
        var album = NewAlbum("Multi Genre Pick", isStaffPick: true);
        Link(album, NewGenre("Genre One", "genre-one"));
        Link(album, NewGenre("Genre Two", "genre-two"));
        DbContext.Albums.Add(album);
        await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await HandlerOn(s_epoch)
            .Handle(new GetCoverStoryQuery(), CancellationToken.None);

        result.Value.Genres.Select(g => g.Name).Should().BeEquivalentTo("Genre One", "Genre Two");
    }

    private static void Link(Album album, Genre genre) =>
        album.AlbumGenres.Add(new AlbumGenre { Album = album, Genre = genre });

    private static Genre NewGenre(string name, string slug) => new() { Name = name, Slug = slug };

    private static Album NewAlbum(
        string title,
        DateOnly? releaseDate = null,
        bool isStaffPick = false,
        int unitsInStock = 0
    ) =>
        new()
        {
            Title = title,
            PriceInPence = 1000,
            ReleaseDate = releaseDate,
            IsStaffPick = isStaffPick,
            UnitsInStock = unitsInStock,
        };
}
