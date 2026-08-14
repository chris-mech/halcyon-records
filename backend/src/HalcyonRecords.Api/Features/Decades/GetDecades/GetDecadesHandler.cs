using ErrorOr;
using HalcyonRecords.Api.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HalcyonRecords.Api.Features.Decades.GetDecades;

public sealed class GetDecadesHandler(ApplicationDbContext dbContext)
    : IRequestHandler<GetDecadesQuery, ErrorOr<IReadOnlyList<DecadeListItemResponse>>>
{
    public async Task<ErrorOr<IReadOnlyList<DecadeListItemResponse>>> Handle(
        GetDecadesQuery query,
        CancellationToken cancellationToken
    )
    {
        return await dbContext
            .Decades.OrderByDescending(d => d.StartYear ?? int.MinValue)
            .Select(d => new DecadeListItemResponse(
                d.Slug,
                d.Label,
                d.StartYear,
                d.EndYear,
                d.ImageUrl,
                dbContext.Albums.Count(a =>
                    a.ReleaseDate != null
                    && (d.StartYear == null || a.ReleaseDate.Value.Year >= d.StartYear)
                    && (d.EndYear == null || a.ReleaseDate.Value.Year <= d.EndYear)
                )
            ))
            .ToListAsync(cancellationToken);
    }
}
