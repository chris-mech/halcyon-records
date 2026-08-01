using System.Text.Json;
using ErrorOr;
using HalcyonRecords.SeedDataGenerator.Core.Enrichment;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.Core.Store;

public sealed class SeedDataStore
{
    private readonly IArtistEnricher _artistEnricher;
    private readonly IAlbumEnricher _albumEnricher;
    private readonly SeedDataStoreOptions _options;

    private readonly Dictionary<Guid, ArtistSeedEntry> _artistsBySourceId = [];
    private readonly Dictionary<string, GenreSeedEntry> _genresBySlug = [];
    private readonly Dictionary<Guid, AlbumSeedEntry> _albumsBySourceId = [];

    public SeedDataStore(
        IArtistEnricher artistEnricher,
        IAlbumEnricher albumEnricher,
        SeedDataStoreOptions options
    )
    {
        _artistEnricher = artistEnricher;
        _albumEnricher = albumEnricher;
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

        return await _artistEnricher.PreviewAsync(artistMbid, cancellationToken);
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

        return await _albumEnricher.PreviewAsync(
            releaseMbid,
            _artistsBySourceId.Keys.ToHashSet(),
            cancellationToken
        );
    }

    public Task<AddAlbumPlan> ResolveDiscogsMasterAsync(
        AddAlbumPlan plan,
        long chosenMasterId,
        CancellationToken cancellationToken = default
    ) => _albumEnricher.ResolveDiscogsMasterAsync(plan, chosenMasterId, cancellationToken);

    public AlbumSeedEntry CommitAddAlbum(AddAlbumPlan plan)
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

            var missingArtistPlan = plan.MissingArtistPlans.FirstOrDefault(p =>
                p.SourceId == artistId
            );
            if (missingArtistPlan is null)
            {
                continue;
            }

            CommitAddArtist(missingArtistPlan);
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
