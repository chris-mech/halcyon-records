namespace HalcyonRecords.Api.Features.Genres.GetGenres;

public sealed record GenreListItemResponse(
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int AlbumCount
);
