using ErrorOr;
using HalcyonRecords.Api.Common.Caching;
using MediatR;

namespace HalcyonRecords.Api.Features.Artists.GetArtistById;

public sealed record GetArtistByIdQuery(string Sqid, string Sort)
    : IRequest<ErrorOr<ArtistDetailResponse>>,
        ICacheableQuery
{
    public string CacheKey => $"artists:byId:{Sqid}:sort={Sort}";

    public IReadOnlyCollection<string> Tags => ["albums"];
}
