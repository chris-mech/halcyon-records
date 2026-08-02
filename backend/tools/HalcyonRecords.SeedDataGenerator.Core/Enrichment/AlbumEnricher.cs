using ErrorOr;
using HalcyonRecords.SeedDataGenerator.Core.Discogs;
using HalcyonRecords.SeedDataGenerator.Core.Services;

namespace HalcyonRecords.SeedDataGenerator.Core.Enrichment;

public interface IAlbumEnricher
{
    Task<ErrorOr<AddAlbumPlan>> PreviewAsync(
        Guid releaseMbid,
        IReadOnlySet<Guid> knownArtistSourceIds,
        CancellationToken cancellationToken = default
    );

    Task<AddAlbumPlan> ResolveDiscogsMasterAsync(
        AddAlbumPlan plan,
        long chosenMasterId,
        CancellationToken cancellationToken = default
    );
}

public sealed record AddAlbumPlan(
    Guid SourceId,
    string Title,
    DateOnly? ReleaseDate,
    string? Label,
    IReadOnlyList<Guid> ArtistCreditIds,
    long? DiscogsMasterId,
    IReadOnlyList<DiscogsSearchResult>? DiscogsCandidates,
    IReadOnlyList<ResolvedGenre> ResolvedGenres,
    Uri? CoverImageUrl,
    string? Description,
    IReadOnlyList<AddArtistPlan> MissingArtistPlans
);

public sealed class AlbumEnricher(
    IReleaseService releaseService,
    IReleaseGroupService releaseGroupService,
    IGenreService genreService,
    ICoverImageService coverImageService,
    IDescriptionService descriptionService,
    DiscogsClient discogsClient,
    IArtistEnricher artistEnricher
) : IAlbumEnricher
{
    public async Task<ErrorOr<AddAlbumPlan>> PreviewAsync(
        Guid releaseMbid,
        IReadOnlySet<Guid> knownArtistSourceIds,
        CancellationToken cancellationToken = default
    )
    {
        var loaded = await releaseService.LoadAsync(releaseMbid, cancellationToken);
        if (loaded.IsError)
        {
            return loaded.Errors;
        }

        var fields = loaded.Value;

        var coverImageUrl = await coverImageService.ResolveAsync(
            releaseMbid,
            fields.ReleaseGroupId,
            cancellationToken
        );

        var releaseGroupFields = fields.ReleaseGroupId is { } releaseGroupId
            ? await releaseGroupService.ResolveAsync(releaseGroupId, cancellationToken)
            : null;

        var discogsMasterId = releaseGroupFields?.DiscogsMasterId;
        IReadOnlyList<ResolvedGenre> resolvedGenres = [];
        IReadOnlyList<DiscogsSearchResult>? discogsCandidates = null;

        if (discogsMasterId is not null)
        {
            resolvedGenres = await genreService.ResolveAsync(
                discogsMasterId.Value,
                cancellationToken
            );
        }
        else
        {
            discogsCandidates = fields.PrimaryArtistName is not null
                ? await discogsClient.SearchMastersAsync(
                    fields.PrimaryArtistName,
                    fields.Title,
                    cancellationToken
                )
                : [];
        }

        var description = await descriptionService.ResolveAsync(
            releaseGroupFields?.WikidataQid,
            cancellationToken
        );

        List<AddArtistPlan> missingArtistPlans = [];
        foreach (var artistId in fields.ArtistCreditIds)
        {
            if (knownArtistSourceIds.Contains(artistId))
            {
                continue;
            }

            var artistPlan = await artistEnricher.PreviewAsync(artistId, cancellationToken);
            if (!artistPlan.IsError)
            {
                missingArtistPlans.Add(artistPlan.Value);
            }
        }

        return new AddAlbumPlan(
            fields.SourceId,
            fields.Title,
            fields.ReleaseDate,
            fields.Label,
            fields.ArtistCreditIds,
            discogsMasterId,
            discogsCandidates,
            resolvedGenres,
            coverImageUrl,
            description,
            missingArtistPlans
        );
    }

    public async Task<AddAlbumPlan> ResolveDiscogsMasterAsync(
        AddAlbumPlan plan,
        long chosenMasterId,
        CancellationToken cancellationToken = default
    )
    {
        var resolvedGenres = await genreService.ResolveAsync(chosenMasterId, cancellationToken);

        return plan with
        {
            DiscogsMasterId = chosenMasterId,
            DiscogsCandidates = null,
            ResolvedGenres = resolvedGenres,
        };
    }
}
