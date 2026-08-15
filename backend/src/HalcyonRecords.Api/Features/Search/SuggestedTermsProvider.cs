using HalcyonRecords.Api.Features.Search.Search;
using HalcyonRecords.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HalcyonRecords.Api.Features.Search;

public sealed class SuggestedTermsProvider(IOptions<SearchOptions> searchOptions)
{
    public async Task<IReadOnlyList<string>> GetRandomTermsAsync(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken
    )
    {
        var count = searchOptions.Value.SuggestedTermCount;

        var titleCandidates = await dbContext
            .Albums.OrderBy(_ => Guid.NewGuid())
            .Select(a => a.Title)
            .Take(count)
            .ToListAsync(cancellationToken);

        var artistCandidates = await dbContext
            .Artists.OrderBy(_ => Guid.NewGuid())
            .Select(a => a.Name)
            .Take(count)
            .ToListAsync(cancellationToken);

        var genreCandidates = await dbContext
            .Genres.OrderBy(_ => Guid.NewGuid())
            .Select(g => g.Name)
            .Take(count)
            .ToListAsync(cancellationToken);

        return titleCandidates
            .Concat(artistCandidates)
            .Concat(genreCandidates)
            .OrderBy(_ => Guid.NewGuid())
            .Take(count)
            .ToList();
    }
}
