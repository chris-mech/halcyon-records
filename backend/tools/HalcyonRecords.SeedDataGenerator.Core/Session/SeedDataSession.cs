using System.Text.Json;
using ErrorOr;
using HalcyonRecords.SeedDataGenerator.Core.Common;
using HalcyonRecords.SeedDataGenerator.Core.Discogs;
using HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;
using HalcyonRecords.SeedDataGenerator.Core.Services;
using HalcyonRecords.Shared;

namespace HalcyonRecords.SeedDataGenerator.Core.Session;

public sealed record AddArtistPlan(
    ArtistMbid SourceId,
    string Name,
    string? Origin,
    ArtistType? Type,
    int? SinceYear,
    string? Bio,
    string? ImageUrl
);

public sealed record AddAlbumPlan(
    ReleaseMbid SourceId,
    string Title,
    DateOnly? ReleaseDate,
    string? Label,
    IReadOnlyList<ArtistMbid> ArtistCreditIds,
    DiscogsMasterId? DiscogsMasterId,
    IReadOnlyList<DiscogsSearchResult>? DiscogsCandidates,
    IReadOnlyList<ResolvedGenre> ResolvedGenres,
    Uri? CoverImageUrl,
    string? Description,
    IReadOnlyList<AddArtistPlan> MissingArtistPlans
);

public sealed record AlbumCommerceDetails(
    int UnitsInStock,
    int PriceInPence,
    int? OriginalPriceInPence,
    bool IsNew,
    bool IsStaffPick
);

public sealed record AlbumLinks(
    IReadOnlyList<ArtistMbid> ArtistSourceIds,
    IReadOnlyList<GenreSlug> GenreSlugs
);

public enum SeedMode
{
    Merge,
    Overwrite,
}

public sealed class SeedDataSession(
    IReleaseService releaseService,
    IReleaseGroupService releaseGroupService,
    IMusicBrainzArtistService musicBrainzArtistService,
    IDiscogsArtistService discogsArtistService,
    IGenreService genreService,
    ICoverImageService coverImageService,
    IDescriptionService descriptionService,
    IDiscogsMasterSearchService discogsMasterSearchService,
    IMusicBrainzArtistSearchService musicBrainzArtistSearchService,
    IMusicBrainzReleaseSearchService musicBrainzReleaseSearchService,
    SeedDataSessionOptions options
)
{
    private readonly Dictionary<ArtistMbid, ArtistSeedEntry> _artistsBySourceId = [];
    private readonly Dictionary<GenreSlug, GenreSeedEntry> _genresBySlug = [];
    private readonly Dictionary<ReleaseMbid, AlbumSeedEntry> _albumsBySourceId = [];

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

    public Task<IReadOnlyList<MusicBrainzArtistSearchResult>> SearchArtistsAsync(
        string name,
        CancellationToken cancellationToken = default
    ) => musicBrainzArtistSearchService.ResolveAsync(name, cancellationToken);

    public Task<IReadOnlyList<MusicBrainzReleaseSearchResult>> SearchAlbumsAsync(
        string? artistName,
        string releaseTitle,
        CancellationToken cancellationToken = default
    ) => musicBrainzReleaseSearchService.ResolveAsync(artistName, releaseTitle, cancellationToken);

    public async Task<ErrorOr<AddArtistPlan>> PreviewAddArtistAsync(
        ArtistMbid artistMbid,
        CancellationToken cancellationToken = default
    )
    {
        if (Mode == SeedMode.Merge && _artistsBySourceId.ContainsKey(artistMbid))
        {
            return DomainErrors.Artist.AlreadySeeded(artistMbid);
        }

        return await LoadArtistPlanAsync(artistMbid, cancellationToken);
    }

    public ArtistSeedEntry CommitAddArtist(AddArtistPlan plan)
    {
        var entry = new ArtistSeedEntry(
            Name: plan.Name,
            SourceId: plan.SourceId,
            Source: SeedSource.MusicBrainz,
            Bio: plan.Bio,
            Origin: plan.Origin,
            Type: plan.Type,
            SinceYear: plan.SinceYear,
            ImageUrl: plan.ImageUrl
        );

        _artistsBySourceId[entry.SourceId] = entry;
        return entry;
    }

    public IReadOnlyList<AlbumSeedEntry> GetAlbumsReferencing(ArtistMbid artistId) =>
        _albumsBySourceId.Values.Where(album => album.ArtistSourceIds.Contains(artistId)).ToList();

    public IReadOnlyList<AlbumSeedEntry> GetAlbumsReferencing(GenreSlug genreSlug) =>
        _albumsBySourceId.Values.Where(album => album.GenreSlugs.Contains(genreSlug)).ToList();

    public IReadOnlyList<AlbumSeedEntry> DeleteArtist(ArtistMbid artistId)
    {
        if (!_artistsBySourceId.ContainsKey(artistId))
        {
            throw new InvalidOperationException(
                $"Cannot delete artist '{artistId}': no such artist in the session."
            );
        }

        var affectedAlbums = GetAlbumsReferencing(artistId);

        foreach (var album in affectedAlbums)
        {
            _albumsBySourceId.Remove(album.SourceId);
        }

        _artistsBySourceId.Remove(artistId);

        return affectedAlbums;
    }

    public Task<ErrorOr<AddArtistPlan>> RefreshArtistAsync(
        ArtistMbid artistId,
        CancellationToken cancellationToken = default
    ) => LoadArtistPlanAsync(artistId, cancellationToken);

    public ArtistSeedEntry UpdateArtist(AddArtistPlan plan)
    {
        if (!_artistsBySourceId.ContainsKey(plan.SourceId))
        {
            throw new InvalidOperationException(
                $"Cannot update artist '{plan.SourceId}': no such artist in the session."
            );
        }

        var updated = _artistsBySourceId[plan.SourceId] with
        {
            Name = plan.Name,
            Origin = plan.Origin,
            Type = plan.Type,
            SinceYear = plan.SinceYear,
            Bio = plan.Bio,
            ImageUrl = plan.ImageUrl,
        };

        _artistsBySourceId[plan.SourceId] = updated;
        return updated;
    }

    public async Task<ErrorOr<AddAlbumPlan>> PreviewAddAlbumAsync(
        ReleaseMbid releaseMbid,
        CancellationToken cancellationToken = default
    )
    {
        if (Mode == SeedMode.Merge && _albumsBySourceId.ContainsKey(releaseMbid))
        {
            return DomainErrors.Album.AlreadySeeded(releaseMbid);
        }

        return await LoadAlbumPlanAsync(releaseMbid, cancellationToken);
    }

    public async Task<AddAlbumPlan> ResolveDiscogsMasterAsync(
        AddAlbumPlan plan,
        DiscogsMasterId chosenMasterId,
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

    public AlbumSeedEntry CommitAddAlbum(AddAlbumPlan plan, AlbumCommerceDetails commerce)
    {
        List<GenreSlug> genreSlugs = [];
        foreach (var genre in plan.ResolvedGenres)
        {
            _genresBySlug.TryAdd(genre.Slug, new GenreSeedEntry(genre.Name, genre.Slug));
            genreSlugs.Add(genre.Slug);
        }

        List<ArtistMbid> artistSourceIds = [];
        foreach (var artistId in plan.ArtistCreditIds)
        {
            if (!_artistsBySourceId.ContainsKey(artistId))
            {
                var missingArtistPlan = plan.MissingArtistPlans.FirstOrDefault(p =>
                    p.SourceId == artistId
                );
                if (missingArtistPlan is null)
                {
                    continue;
                }

                CommitAddArtist(missingArtistPlan);
            }

            artistSourceIds.Add(artistId);
        }

        var entry = new AlbumSeedEntry(
            Title: plan.Title,
            SourceId: plan.SourceId,
            Source: SeedSource.MusicBrainz,
            Description: plan.Description,
            ReleaseDate: plan.ReleaseDate,
            Label: plan.Label,
            IsNew: commerce.IsNew,
            IsStaffPick: commerce.IsStaffPick,
            ImageUrl: plan.CoverImageUrl?.ToString(),
            ArtistSourceIds: artistSourceIds,
            GenreSlugs: genreSlugs,
            UnitsInStock: commerce.UnitsInStock,
            PriceInPence: commerce.PriceInPence,
            OriginalPriceInPence: commerce.OriginalPriceInPence
        );

        _albumsBySourceId[entry.SourceId] = entry;
        return entry;
    }

    public AlbumSeedEntry UpdateAlbum(
        ReleaseMbid sourceId,
        string title,
        DateOnly? releaseDate,
        string? label,
        string? description,
        string? imageUrl,
        AlbumLinks links,
        AlbumCommerceDetails commerce
    )
    {
        if (!_albumsBySourceId.ContainsKey(sourceId))
        {
            throw new InvalidOperationException(
                $"Cannot update album '{sourceId}': no such album in the session."
            );
        }

        var updated = _albumsBySourceId[sourceId] with
        {
            Title = title,
            ReleaseDate = releaseDate,
            Label = label,
            Description = description,
            ImageUrl = imageUrl,
            ArtistSourceIds = links.ArtistSourceIds,
            GenreSlugs = links.GenreSlugs,
            UnitsInStock = commerce.UnitsInStock,
            PriceInPence = commerce.PriceInPence,
            OriginalPriceInPence = commerce.OriginalPriceInPence,
            IsNew = commerce.IsNew,
            IsStaffPick = commerce.IsStaffPick,
        };

        _albumsBySourceId[sourceId] = updated;
        return updated;
    }

    public void DeleteAlbum(ReleaseMbid sourceId)
    {
        if (!_albumsBySourceId.Remove(sourceId))
        {
            throw new InvalidOperationException(
                $"Cannot delete album '{sourceId}': no such album in the session."
            );
        }
    }

    public Task<ErrorOr<AddAlbumPlan>> RefreshAlbumAsync(
        ReleaseMbid sourceId,
        CancellationToken cancellationToken = default
    ) => LoadAlbumPlanAsync(sourceId, cancellationToken);

    public IReadOnlyList<string> GetAvailableGenreNames() =>
        GenreService.KnownGenreNames.Except(_genresBySlug.Values.Select(g => g.Name)).ToList();

    public GenreSeedEntry AddGenre(string name)
    {
        if (!GenreService.KnownGenreNames.Contains(name))
        {
            throw new InvalidOperationException(
                $"Cannot add genre '{name}': not part of the known Discogs taxonomy."
            );
        }

        var slug = new GenreSlug(Slugifier.Slugify(name));

        if (!_genresBySlug.TryAdd(slug, new GenreSeedEntry(name, slug)))
        {
            throw new InvalidOperationException(
                $"Cannot add genre '{name}': already present in the session."
            );
        }

        return _genresBySlug[slug];
    }

    public GenreSeedEntry UpdateGenreDescription(GenreSlug slug, string? description)
    {
        if (!_genresBySlug.ContainsKey(slug))
        {
            throw new InvalidOperationException(
                $"Cannot update genre '{slug}': no such genre in the session."
            );
        }

        var updated = _genresBySlug[slug] with { Description = description };
        _genresBySlug[slug] = updated;
        return updated;
    }

    public IReadOnlyList<AlbumSeedEntry> DeleteGenre(GenreSlug slug)
    {
        if (!_genresBySlug.ContainsKey(slug))
        {
            throw new InvalidOperationException(
                $"Cannot delete genre '{slug}': no such genre in the session."
            );
        }

        var affectedAlbums = GetAlbumsReferencing(slug);

        foreach (var album in affectedAlbums)
        {
            _albumsBySourceId[album.SourceId] = album with
            {
                GenreSlugs = album.GenreSlugs.Where(genreSlug => genreSlug != slug).ToList(),
            };
        }

        _genresBySlug.Remove(slug);

        return affectedAlbums;
    }

    private async Task<ErrorOr<AddAlbumPlan>> LoadAlbumPlanAsync(
        ReleaseMbid releaseMbid,
        CancellationToken cancellationToken
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

        var description = await descriptionService.ResolveAsync(
            releaseGroupFields?.WikidataQid,
            cancellationToken
        );

        IReadOnlyList<ResolvedGenre> resolvedGenres = [];
        IReadOnlyList<DiscogsSearchResult>? discogsCandidates = null;

        if (discogsMasterId is { } masterId)
        {
            resolvedGenres = await genreService.ResolveAsync(masterId, cancellationToken);
        }
        else
        {
            discogsCandidates = await discogsMasterSearchService.ResolveAsync(
                fields.PrimaryArtistName,
                fields.Title,
                cancellationToken
            );
        }

        var missingArtistPlans = await ResolveMissingArtistPlansAsync(
            fields.ArtistCreditIds,
            cancellationToken
        );

        return new AddAlbumPlan(
            SourceId: fields.SourceId,
            Title: fields.Title,
            ReleaseDate: fields.ReleaseDate,
            Label: fields.Label,
            ArtistCreditIds: fields.ArtistCreditIds,
            DiscogsMasterId: discogsMasterId,
            DiscogsCandidates: discogsCandidates,
            ResolvedGenres: resolvedGenres,
            CoverImageUrl: coverImageUrl,
            Description: description,
            MissingArtistPlans: missingArtistPlans
        );
    }

    private async Task<List<AddArtistPlan>> ResolveMissingArtistPlansAsync(
        IReadOnlyList<ArtistMbid> artistCreditIds,
        CancellationToken cancellationToken
    )
    {
        List<AddArtistPlan> plans = [];
        foreach (var artistId in artistCreditIds)
        {
            if (_artistsBySourceId.ContainsKey(artistId))
            {
                continue;
            }

            var artistPlan = await LoadArtistPlanAsync(artistId, cancellationToken);
            if (!artistPlan.IsError)
            {
                plans.Add(artistPlan.Value);
            }
        }

        return plans;
    }

    private async Task<ErrorOr<AddArtistPlan>> LoadArtistPlanAsync(
        ArtistMbid artistMbid,
        CancellationToken cancellationToken
    )
    {
        var loaded = await musicBrainzArtistService.LoadAsync(artistMbid, cancellationToken);
        if (loaded.IsError)
        {
            return loaded.Errors;
        }

        var fields = loaded.Value;

        var discogsFields = await discogsArtistService.ResolveAsync(
            fields.DiscogsArtistId,
            cancellationToken
        );

        var bio = discogsFields.Bio;
        if (string.IsNullOrWhiteSpace(bio))
        {
            bio = await descriptionService.ResolveAsync(fields.WikidataQid, cancellationToken);
        }

        return new AddArtistPlan(
            SourceId: fields.SourceId,
            Name: fields.Name,
            Origin: fields.Origin,
            Type: fields.Type,
            SinceYear: fields.SinceYear,
            Bio: bio,
            ImageUrl: discogsFields.ImageUrl
        );
    }

    private async Task<List<T>> ReadSeedFileAsync<T>(
        string fileName,
        CancellationToken cancellationToken
    )
    {
        var filePath = Path.Combine(options.SeedDataFolder, fileName);

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
        var filePath = Path.Combine(options.SeedDataFolder, fileName);
        await using var stream = File.Create(filePath);

        await JsonSerializer.SerializeAsync(
            stream,
            values,
            SeedDataJsonOptions.Default,
            cancellationToken
        );
    }
}
