using ErrorOr;
using HalcyonRecords.Api.Common.Caching;
using MediatR;

namespace HalcyonRecords.Api.Features.Artists.GetArtists;

public sealed record GetArtistsQuery
    : IRequest<ErrorOr<IReadOnlyList<ArtistListItemResponse>>>,
        ICacheableQuery
{
    public string CacheKey => "artists:list";

    public IReadOnlyCollection<string> Tags => [];
}
