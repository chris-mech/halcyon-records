using ErrorOr;
using HalcyonRecords.Api.Infrastructure.Sql;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Genres.GetGenreBySlug;

public sealed class GetGenreBySlugHandler(ApplicationDbContext dbContext)
    : IRequestHandler<GetGenreBySlugQuery, ErrorOr<GenreDetailResponse>>
{
    public async Task<ErrorOr<GenreDetailResponse>> Handle(
        GetGenreBySlugQuery query,
        CancellationToken cancellationToken
    )
    {
        var genre = await dbContext
            .Genres.Where(g => g.Slug == query.Slug)
            .Select(g => new GenreDetailResponse(
                g.Name,
                g.Slug,
                g.Description,
                g.ImageUrl,
                g.AlbumGenres.Count
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return genre is null
            ? DomainErrors.Genre.NotFound($"Genre '{query.Slug}' was not found.")
            : genre;
    }
}
