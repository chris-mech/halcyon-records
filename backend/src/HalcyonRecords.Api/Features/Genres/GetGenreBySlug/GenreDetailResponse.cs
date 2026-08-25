namespace HalcyonRecords.Api.Features.Genres.GetGenreBySlug;

public sealed record GenreDetailResponse(
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int AlbumCount
);
