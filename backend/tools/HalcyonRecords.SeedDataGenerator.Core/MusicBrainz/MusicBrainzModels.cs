namespace HalcyonRecords.SeedDataGenerator.Core.MusicBrainz;

public sealed record MusicBrainzRelease(
    Guid? Id,
    string? Title,
    string? Date,
    IReadOnlyList<MusicBrainzLabelInfo>? LabelInfo,
    IReadOnlyList<MusicBrainzArtistCredit>? ArtistCredit,
    MusicBrainzReleaseGroupRef? ReleaseGroup
);

public sealed record MusicBrainzLabelInfo(MusicBrainzLabel? Label);

public sealed record MusicBrainzLabel(string? Name);

public sealed record MusicBrainzArtistCredit(string? Name, MusicBrainzArtistRef? Artist);

public sealed record MusicBrainzArtistRef(Guid? Id, string? Name);

public sealed record MusicBrainzReleaseGroupRef(Guid? Id);

public sealed record MusicBrainzArtist(
    Guid? Id,
    string? Name,
    string? Type,
    MusicBrainzLifeSpan? LifeSpan,
    MusicBrainzArea? Area,
    IReadOnlyList<MusicBrainzRelation>? Relations
);

public sealed record MusicBrainzLifeSpan(string? Begin);

public sealed record MusicBrainzArea(string? Name);

public sealed record MusicBrainzReleaseGroup(
    Guid? Id,
    IReadOnlyList<MusicBrainzRelation>? Relations
);

public sealed record MusicBrainzRelation(string? Type, MusicBrainzUrl? Url);

public sealed record MusicBrainzUrl(string? Resource);

public sealed record MusicBrainzArtistSearchResponse(
    IReadOnlyList<MusicBrainzArtistSearchResult>? Artists
);

public sealed record MusicBrainzArtistSearchResult(
    Guid? Id,
    string? Name,
    int? Score,
    string? Disambiguation
);

public sealed record MusicBrainzReleaseSearchResponse(
    IReadOnlyList<MusicBrainzReleaseSearchResult>? Releases
);

public sealed record MusicBrainzReleaseSearchResult(
    Guid? Id,
    string? Title,
    int? Score,
    string? Date,
    IReadOnlyList<MusicBrainzArtistCredit>? ArtistCredit
);
