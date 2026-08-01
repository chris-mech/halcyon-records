using System.Text.Json;
using ErrorOr;
using HalcyonRecords.Api.Common.Results;
using HalcyonRecords.Api.Infrastructure.Seed;
using HalcyonRecords.SeedDataGenerator.Core.CoverArtArchive;
using HalcyonRecords.SeedDataGenerator.Core.Discogs;
using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.SeedDataGenerator.Core.Parsing;
using HalcyonRecords.SeedDataGenerator.Core.Wikidata;
using HalcyonRecords.SeedDataGenerator.Core.Wikipedia;

namespace HalcyonRecords.SeedDataGenerator.Core.Store;

public sealed class SeedDataStore
{
    private readonly MusicBrainzClient _musicBrainzClient;
    private readonly DiscogsClient _discogsClient;
    private readonly CoverArtArchiveClient _coverArtArchiveClient;
    private readonly WikidataClient _wikidataClient;
    private readonly WikipediaClient _wikipediaClient;
    private readonly SeedDataStoreOptions _options;

    private readonly Dictionary<Guid, ArtistSeedEntry> _artistsBySourceId = [];
    private readonly Dictionary<string, GenreSeedEntry> _genresBySlug = [];
    private readonly Dictionary<Guid, AlbumSeedEntry> _albumsBySourceId = [];

    public SeedDataStore(
        MusicBrainzClient musicBrainzClient,
        DiscogsClient discogsClient,
        CoverArtArchiveClient coverArtArchiveClient,
        WikidataClient wikidataClient,
        WikipediaClient wikipediaClient,
        SeedDataStoreOptions options
    )
    {
        _musicBrainzClient = musicBrainzClient;
        _discogsClient = discogsClient;
        _coverArtArchiveClient = coverArtArchiveClient;
        _wikidataClient = wikidataClient;
        _wikipediaClient = wikipediaClient;
        _options = options;
    }

    public SeedMode Mode { get; private set; } = SeedMode.Merge;

    public IReadOnlyCollection<ArtistSeedEntry> Artists => _artistsBySourceId.Values;
    public IReadOnlyCollection<GenreSeedEntry> Genres => _genresBySlug.Values;
    public IReadOnlyCollection<AlbumSeedEntry> Albums => _albumsBySourceId.Values;

    public async Task LoadAsync(SeedMode mode, CancellationToken cancellationToken = default)
    {
        Mode = mode;

        _artistsBySourceId.Clear();
        foreach (
            var artist in await ReadSeedFileAsync<ArtistSeedEntry>(
                SeedDataFileNames.Artists,
                cancellationToken
            )
        )
        {
            _artistsBySourceId[artist.SourceId] = artist;
        }

        _genresBySlug.Clear();
        foreach (
            var genre in await ReadSeedFileAsync<GenreSeedEntry>(
                SeedDataFileNames.Genres,
                cancellationToken
            )
        )
        {
            _genresBySlug[genre.Slug] = genre;
        }

        _albumsBySourceId.Clear();
        foreach (
            var album in await ReadSeedFileAsync<AlbumSeedEntry>(
                SeedDataFileNames.Albums,
                cancellationToken
            )
        )
        {
            _albumsBySourceId[album.SourceId] = album;
        }
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await WriteSeedFileAsync(
            SeedDataFileNames.Artists,
            _artistsBySourceId.Values,
            cancellationToken
        );
        await WriteSeedFileAsync(SeedDataFileNames.Genres, _genresBySlug.Values, cancellationToken);
        await WriteSeedFileAsync(
            SeedDataFileNames.Albums,
            _albumsBySourceId.Values,
            cancellationToken
        );
    }

    public async Task<ErrorOr<AddArtistPlan>> PreviewAddArtistAsync(
        Guid artistMbid,
        CancellationToken cancellationToken = default
    )
    {
        if (Mode == SeedMode.Merge && _artistsBySourceId.ContainsKey(artistMbid))
        {
            return DomainErrors.Artist.AlreadySeeded(artistMbid);
        }

        var raw = await _musicBrainzClient.GetArtistAsync(artistMbid, cancellationToken);
        if (raw is null)
        {
            return DomainErrors.Artist.NotFound($"No MusicBrainz artist found for '{artistMbid}'.");
        }

        var parsed = MusicBrainzParsing.ParseArtist(raw);
        if (parsed.IsError)
        {
            return parsed.Errors;
        }

        var fields = parsed.Value;

        var discogsArtist = fields.DiscogsArtistId is not null
            ? await _discogsClient.GetArtistAsync(fields.DiscogsArtistId.Value, cancellationToken)
            : null;
        var discogsFields = DiscogsParsing.ParseArtist(discogsArtist);

        var bio = discogsFields.Bio;
        if (string.IsNullOrWhiteSpace(bio))
        {
            bio = await ResolveWikipediaExtractAsync(fields.WikidataQid, cancellationToken);
        }

        return new AddArtistPlan(
            fields.SourceId,
            fields.Name,
            fields.Origin,
            fields.ActiveSince,
            bio,
            discogsFields.ImageUrl
        );
    }

    public ArtistSeedEntry CommitAddArtist(AddArtistPlan plan)
    {
        var entry = new ArtistSeedEntry(
            Name: plan.Name,
            SourceId: plan.SourceId,
            Source: SeedSource.MusicBrainz,
            Bio: plan.Bio,
            Origin: plan.Origin,
            ActiveSince: plan.ActiveSince,
            ImageUrl: plan.ImageUrl
        );

        _artistsBySourceId[entry.SourceId] = entry;
        return entry;
    }

    public async Task<ErrorOr<AddAlbumPlan>> PreviewAddAlbumAsync(
        Guid releaseMbid,
        CancellationToken cancellationToken = default
    )
    {
        if (Mode == SeedMode.Merge && _albumsBySourceId.ContainsKey(releaseMbid))
        {
            return DomainErrors.Album.AlreadySeeded(releaseMbid);
        }

        var raw = await _musicBrainzClient.GetReleaseAsync(releaseMbid, cancellationToken);
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
            ? await _musicBrainzClient.GetReleaseGroupAsync(releaseGroupId, cancellationToken)
            : null;

        var coverImageUrl = await _coverArtArchiveClient.GetReleaseFrontImageUrlAsync(
            releaseMbid,
            cancellationToken
        );
        if (coverImageUrl is null && fields.ReleaseGroupId is { } fallbackGroupId)
        {
            coverImageUrl = await _coverArtArchiveClient.GetReleaseGroupFrontImageUrlAsync(
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
            var primaryArtistName = raw.ArtistCredit?.FirstOrDefault()?.Name;
            discogsCandidates = primaryArtistName is not null
                ? await _discogsClient.SearchMastersAsync(
                    primaryArtistName,
                    fields.Title,
                    cancellationToken
                )
                : [];
        }

        var description = await ResolveWikipediaExtractAsync(
            releaseGroupFields?.WikidataQid,
            cancellationToken
        );

        var missingArtistCreditIds = fields
            .ArtistCreditIds.Where(artistId => !_artistsBySourceId.ContainsKey(artistId))
            .ToList();

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
            missingArtistCreditIds
        );
    }

    public async Task<ErrorOr<AddAlbumPlan>> ResolveDiscogsMasterAsync(
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

    public async Task<AlbumSeedEntry> CommitAddAlbumAsync(
        AddAlbumPlan plan,
        CancellationToken cancellationToken = default
    )
    {
        List<string> genreSlugs = [];
        foreach (var genre in plan.ResolvedGenres)
        {
            if (!_genresBySlug.ContainsKey(genre.Slug))
            {
                _genresBySlug[genre.Slug] = new GenreSeedEntry(genre.Name, genre.Slug);
            }

            genreSlugs.Add(genre.Slug);
        }

        List<Guid> artistSourceIds = [];
        foreach (var artistId in plan.ArtistCreditIds)
        {
            if (_artistsBySourceId.ContainsKey(artistId))
            {
                artistSourceIds.Add(artistId);
                continue;
            }

            var artistPlan = await PreviewAddArtistAsync(artistId, cancellationToken);
            if (artistPlan.IsError)
            {
                continue;
            }

            CommitAddArtist(artistPlan.Value);
            artistSourceIds.Add(artistId);
        }

        var entry = new AlbumSeedEntry(
            Title: plan.Title,
            SourceId: plan.SourceId,
            Source: SeedSource.MusicBrainz,
            Description: plan.Description,
            ReleaseDate: plan.ReleaseDate,
            Label: plan.Label,
            IsNew: false,
            IsStaffPick: false,
            ImageUrl: plan.CoverImageUrl?.ToString(),
            ArtistSourceIds: artistSourceIds,
            GenreSlugs: genreSlugs,
            UnitsInStock: 0,
            PriceInPence: 0,
            OriginalPriceInPence: null
        );

        _albumsBySourceId[entry.SourceId] = entry;
        return entry;
    }

    private async Task<IReadOnlyList<ResolvedGenre>> ResolveGenresAsync(
        long masterId,
        CancellationToken cancellationToken
    )
    {
        var master = await _discogsClient.GetMasterAsync(masterId, cancellationToken);
        return DiscogsParsing.ParseMasterGenres(master);
    }

    private async Task<string?> ResolveWikipediaExtractAsync(
        string? qid,
        CancellationToken cancellationToken
    )
    {
        if (qid is null)
        {
            return null;
        }

        var entity = await _wikidataClient.GetEntityAsync(qid, cancellationToken);
        var title = WikidataParsing.ParseSitelinkTitle(entity);

        if (title is null)
        {
            return null;
        }

        var summary = await _wikipediaClient.GetSummaryAsync(title, cancellationToken);
        return WikipediaParsing.ParseExtract(summary);
    }

    private async Task<List<T>> ReadSeedFileAsync<T>(
        string fileName,
        CancellationToken cancellationToken
    )
    {
        var filePath = Path.Combine(_options.SeedDataFolder, fileName);

        if (!File.Exists(filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(filePath);

        return await JsonSerializer.DeserializeAsync<List<T>>(
                stream,
                SeedDataJsonOptions.Default,
                cancellationToken
            )
            ?? throw new InvalidOperationException($"Seed file '{fileName}' deserialised to null.");
    }

    private async Task WriteSeedFileAsync<T>(
        string fileName,
        IEnumerable<T> values,
        CancellationToken cancellationToken
    )
    {
        var filePath = Path.Combine(_options.SeedDataFolder, fileName);
        await using var stream = File.Create(filePath);

        await JsonSerializer.SerializeAsync(
            stream,
            values.ToList(),
            SeedDataJsonOptions.Default,
            cancellationToken
        );
    }
}
