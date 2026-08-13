namespace HalcyonRecords.Api.Features.Artists.GetArtists;

public sealed record ArtistListItemResponse(
    string Sqid,
    string Name,
    string NameSlug,
    int AlbumCount
);
