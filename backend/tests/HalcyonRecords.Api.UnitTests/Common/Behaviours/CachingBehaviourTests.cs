using ErrorOr;
using FluentAssertions;
using HalcyonRecords.Api.Common.Behaviours;
using HalcyonRecords.Api.Common.Caching;
using HalcyonRecords.Api.Common.Contracts;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace HalcyonRecords.Api.UnitTests.Common.Behaviours;

public class CachingBehaviourTests
{
    public sealed record DummyRequest(string CacheKey)
        : IRequest<ErrorOr<DummyResponse>>,
            ICacheableQuery
    {
        public IReadOnlyCollection<string> Tags => [];
    }

    public sealed record DummyResponse(string Value);

    public sealed record PagedRequest(string CacheKey)
        : IRequest<ErrorOr<PagedResult<DummyResponse>>>,
            ICacheableQuery
    {
        public IReadOnlyCollection<string> Tags => [];
    }

    public sealed record ListRequest(string CacheKey)
        : IRequest<ErrorOr<IReadOnlyList<DummyResponse>>>,
            ICacheableQuery
    {
        public IReadOnlyCollection<string> Tags => [];
    }

    public sealed record TaggedRequest(string CacheKey, string Tag)
        : IRequest<ErrorOr<DummyResponse>>,
            ICacheableQuery
    {
        public IReadOnlyCollection<string> Tags => [Tag];
    }

    private static HybridCache NewCache()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        return services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }

    [Fact]
    public async Task Handle_CachesSuccessResult_AcrossCalls()
    {
        var behaviour = new CachingBehaviour<DummyRequest, DummyResponse>(NewCache());
        var next = Substitute.For<RequestHandlerDelegate<ErrorOr<DummyResponse>>>();
        next.Invoke(Arg.Any<CancellationToken>()).Returns(new DummyResponse("first"));

        var request = new DummyRequest("cache-test-success-key");
        var first = await behaviour.Handle(request, next, CancellationToken.None);
        var second = await behaviour.Handle(request, next, CancellationToken.None);

        first.Value.Should().Be(new DummyResponse("first"));
        second.Value.Should().Be(new DummyResponse("first"));
        await next.Received(1).Invoke(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DoesNotServeErrorAsSuccess_OnCacheHit()
    {
        var behaviour = new CachingBehaviour<DummyRequest, DummyResponse>(NewCache());
        var next = Substitute.For<RequestHandlerDelegate<ErrorOr<DummyResponse>>>();
        next.Invoke(Arg.Any<CancellationToken>())
            .Returns(Error.NotFound(code: "Dummy.NotFound", description: "Not found"));

        var request = new DummyRequest("cache-test-notfound-key");
        await behaviour.Handle(request, next, CancellationToken.None);
        var second = await behaviour.Handle(request, next, CancellationToken.None);

        second.IsError.Should().BeTrue();
        second.FirstError.Code.Should().Be("Dummy.NotFound");
        second.FirstError.Description.Should().Be("Not found");
        second.FirstError.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_RoundTripsPagedResultWithNestedCollections()
    {
        var behaviour = new CachingBehaviour<PagedRequest, PagedResult<DummyResponse>>(NewCache());
        var page = new PagedResult<DummyResponse>(
            [new DummyResponse("a"), new DummyResponse("b")],
            1,
            12,
            2
        );
        var next = Substitute.For<RequestHandlerDelegate<ErrorOr<PagedResult<DummyResponse>>>>();
        next.Invoke(Arg.Any<CancellationToken>()).Returns(page);

        var request = new PagedRequest("cache-test-paged-key");
        await behaviour.Handle(request, next, CancellationToken.None);
        var result = await behaviour.Handle(request, next, CancellationToken.None);

        result.Value.Should().BeEquivalentTo(page);
    }

    [Fact]
    public async Task Handle_RoundTripsListResult()
    {
        var behaviour = new CachingBehaviour<ListRequest, IReadOnlyList<DummyResponse>>(NewCache());
        IReadOnlyList<DummyResponse> items = [new DummyResponse("a"), new DummyResponse("b")];
        var expected = await ErrorOrFactory.FromAsync(items);
        var next = Substitute.For<RequestHandlerDelegate<ErrorOr<IReadOnlyList<DummyResponse>>>>();
        next.Invoke(Arg.Any<CancellationToken>()).Returns(expected);

        var request = new ListRequest("cache-test-list-key");
        await behaviour.Handle(request, next, CancellationToken.None);
        var result = await behaviour.Handle(request, next, CancellationToken.None);

        result.Value.Should().BeEquivalentTo(items);
    }

    [Fact]
    public async Task Handle_EvictsCachedEntry_WhenItsTagIsRemoved()
    {
        var cache = NewCache();
        var behaviour = new CachingBehaviour<TaggedRequest, DummyResponse>(cache);
        var next = Substitute.For<RequestHandlerDelegate<ErrorOr<DummyResponse>>>();
        next.Invoke(Arg.Any<CancellationToken>())
            .Returns(new DummyResponse("first"), new DummyResponse("second"));

        var request = new TaggedRequest("cache-test-tagged-key", "dummy-tag");
        await behaviour.Handle(request, next, CancellationToken.None);
        await cache.RemoveByTagAsync("dummy-tag", TestContext.Current.CancellationToken);
        var afterEviction = await behaviour.Handle(request, next, CancellationToken.None);

        afterEviction.Value.Should().Be(new DummyResponse("second"));
        await next.Received(2).Invoke(Arg.Any<CancellationToken>());
    }
}
