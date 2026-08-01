using ErrorOr;
using HalcyonRecords.SeedDataGenerator.Core.CoverArtArchive;
using HalcyonRecords.SeedDataGenerator.Core.Discogs;
using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.SeedDataGenerator.Core.Parsing;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.Core.Enrichment;

public sealed class AlbumEnricher(
    MusicBrainzClient musicBrainzClient,
    DiscogsClient discogsClient,
    CoverArtArchiveClient coverArtArchiveClient,
    WikipediaDescriptionResolver descriptionResolver,
    IArtistEnricher artistEnricher
) : IAlbumEnricher
{
    public async Task<ErrorOr<AddAlbumPlan>> PreviewAsync(
        Guid releaseMbid,
        IReadOnlySet<Guid> knownArtistSourceIds,
        CancellationToken cancellationToken = default
    )
    {
        var raw = await musicBrainzClient.GetReleaseAsync(releaseMbid, cancellationToken);
        if (raw is null)
        {
            return DomainErrors.Album.NotFound(
                $"No MusicBrainz release found for '{releaseMbid}'."
            );
        }

        var parsed = MusicBrainzParsing.ParseRelease(raw);
        if (parsed.IsError)
        {
            return parsed.Errors;
        }

        var fields = parsed.Value;

        var releaseGroup = fields.ReleaseGroupId is { } releaseGroupId
            ? await musicBrainzClient.GetReleaseGroupAsync(releaseGroupId, cancellationToken)
            : null;

        var coverImageUrl = await coverArtArchiveClient.GetReleaseFrontImageUrlAsync(
            releaseMbid,
            cancellationToken
        );
        if (coverImageUrl is null && fields.ReleaseGroupId is { } fallbackGroupId)
        {
            coverImageUrl = await coverArtArchiveClient.GetReleaseGroupFrontImageUrlAsync(
                fallbackGroupId,
                cancellationToken
            );
        }

        var releaseGroupFields = releaseGroup is not null
            ? MusicBrainzParsing.ParseReleaseGroup(releaseGroup)
            : null;

        var discogsMasterId = releaseGroupFields?.DiscogsMasterId;
        IReadOnlyList<ResolvedGenre> resolvedGenres = [];
        IReadOnlyList<DiscogsSearchResult>? discogsCandidates = null;

        if (discogsMasterId is not null)
        {
            resolvedGenres = await ResolveGenresAsync(discogsMasterId.Value, cancellationToken);
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

        var description = await descriptionResolver.ResolveAsync(
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
        var resolvedGenres = await ResolveGenresAsync(chosenMasterId, cancellationToken);

        return plan with
        {
            DiscogsMasterId = chosenMasterId,
            DiscogsCandidates = null,
            ResolvedGenres = resolvedGenres,
        };
    }

    private async Task<IReadOnlyList<ResolvedGenre>> ResolveGenresAsync(
        long masterId,
        CancellationToken cancellationToken
    )
    {
        var master = await discogsClient.GetMasterAsync(masterId, cancellationToken);
        return DiscogsParsing.ParseMasterGenres(master);
    }
}
