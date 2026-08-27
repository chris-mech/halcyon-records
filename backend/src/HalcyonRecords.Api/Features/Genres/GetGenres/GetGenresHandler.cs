using ErrorOr;
using HalcyonRecords.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Genres.GetGenres;

public sealed class GetGenresHandler(ApplicationDbContext dbContext)
    : IRequestHandler<GetGenresQuery, ErrorOr<IReadOnlyList<GenreListItemResponse>>>
{
    public async Task<ErrorOr<IReadOnlyList<GenreListItemResponse>>> Handle(
        GetGenresQuery query,
        CancellationToken cancellationToken
    )
    {
        return await dbContext
            .Genres.OrderBy(g => g.Name)
            .Select(g => new GenreListItemResponse(g.Name, g.Slug, g.ImageUrl, g.AlbumGenres.Count))
            .ToListAsync(cancellationToken);
    }
}
