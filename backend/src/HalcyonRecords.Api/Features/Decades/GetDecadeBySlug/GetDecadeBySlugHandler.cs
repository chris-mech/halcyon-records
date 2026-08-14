using ErrorOr;
using HalcyonRecords.Api.Infrastructure;
using HalcyonRecords.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Decades.GetDecadeBySlug;

public sealed class GetDecadeBySlugHandler(ApplicationDbContext dbContext)
    : IRequestHandler<GetDecadeBySlugQuery, ErrorOr<DecadeDetailResponse>>
{
    public async Task<ErrorOr<DecadeDetailResponse>> Handle(
        GetDecadeBySlugQuery query,
        CancellationToken cancellationToken
    )
    {
        var decade = await dbContext
            .Decades.Where(d => d.Slug == query.Slug)
            .Select(d => new DecadeDetailResponse(
                d.Slug,
                d.Label,
                d.StartYear,
                d.EndYear,
                d.Description,
                dbContext.Albums.Count(a =>
                    a.ReleaseDate != null
                    && (d.StartYear == null || a.ReleaseDate.Value.Year >= d.StartYear)
                    && (d.EndYear == null || a.ReleaseDate.Value.Year <= d.EndYear)
                )
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return decade is null
            ? DomainErrors.Decade.NotFound($"Decade '{query.Slug}' was not found.")
            : decade;
    }
}
